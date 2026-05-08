using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Bill;
using BackEnd.DTOs.Requests.BillDetail;
using BackEnd.DTOs.Requests.Customer;
using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Services;

public class SalesOrderService(
    AppDbContext context,
    CustomerService customerService,
    StockService stockService,
    BranchService branchService,
    BillService billService,
    BillDetailService billDetailService,
    IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly CustomerService _customerService = customerService;
    private readonly StockService _stockService = stockService;
    private readonly BranchService _branchService = branchService;
    private readonly BillService _billService = billService;
    private readonly BillDetailService _billDetailService = billDetailService;
    private readonly IMapper _mapper = mapper;
    private const int TaxRate = 10;

    public async Task<Result<SalesOrderWrapperDto>> CreateFromPosAsync(CreatePosSaleRequestDto request, int userId)
    {
        if (request.Products.Count == 0)
            return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.DetailsRequired, ErrorType.Validation);

        var customerIdResult = await ResolveCustomerIdAsync(request.Customer);
        if (!customerIdResult.IsSuccess)
            return Result<SalesOrderWrapperDto>.Failure(customerIdResult.ErrorMessage!, customerIdResult.ErrorType);

        var movementType = request.Sale.MovementType ?? (int)BankMovementTypeEnum.Credit;

        var details = new List<CreateSalesOrderDetailRequestDto>();
        foreach (var product in request.Products)
        {
            var productIdResult = await ResolveProductIdAsync(product);
            if (!productIdResult.IsSuccess)
                return Result<SalesOrderWrapperDto>.Failure(productIdResult.ErrorMessage!, productIdResult.ErrorType);

            details.Add(new CreateSalesOrderDetailRequestDto
            {
                ProductId = productIdResult.Value,
                Quantity = product.Quantity
            });
        }

        var isCredit = request.Pay.Condition == PosSaleCondition.Credit;
        var billType = request.Sale.Bill ?? (isCredit ? BillTypeEnum.CREDITO : BillTypeEnum.CONTADO);

        var mappedRequest = new CreateSalesOrderRequestDto
        {
            CustomerId = customerIdResult.Value,
            SalesOrderState = SalesOrderStateEnum.Confirmed,
            Date = request.Sale.Date,
            BillType = billType,
            IsCredit = isCredit,
            PaymentMethod = (PaymentMethodEnum)request.Pay.Method,
            SaleCondition = (SaleConditionEnum)request.Pay.Condition,
            AccountId = request.Sale.AccountId ?? 0,
            MovementType = movementType,
            BranchId = request.Sale.BranchId ?? request.Sale.CashierNumber ?? 0,
            Details = details
        };

        return await CreateAsync(mappedRequest, userId);
    }

    public async Task<Result<SalesOrderWrapperDto>> CreateAsync(CreateSalesOrderRequestDto request, int userId)
    {
        if (request.Details.Count == 0)
            return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.DetailsRequired, ErrorType.Validation);

        var customerResult = await _customerService.GetByIdAsync(request.CustomerId);
        if (!customerResult.IsSuccess)
            return ToSalesOrderFailure(customerResult, SalesOrderError.CustomerNotFound);

        var branchIdResult = await ResolveBranchIdAsync(request.BranchId);
        if (!branchIdResult.IsSuccess)
            return Result<SalesOrderWrapperDto>.Failure(branchIdResult.ErrorMessage!, branchIdResult.ErrorType);

        var accountIdResult = await ResolveAccountIdAsync(request.AccountId);
        if (!accountIdResult.IsSuccess)
            return Result<SalesOrderWrapperDto>.Failure(accountIdResult.ErrorMessage!, accountIdResult.ErrorType);

        var movementType = request.MovementType <= 0
            ? (int)BankMovementTypeEnum.Credit
            : request.MovementType;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Create SalesOrder-------------------------------------------------------------
            var salesOrder = new SalesOrder
            {
                CustomerId = request.CustomerId,
                UserId = userId,
                Number = string.Empty,
                Date = request.Date ?? DateTime.UtcNow,
                SalesOrderState = request.SalesOrderState,
                PaymentMethod = request.PaymentMethod,
                SaleCondition = request.SaleCondition,
                Total = 0 // Will compute
            };

            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            salesOrder.Number = GenerateSalesOrderNumber(salesOrder.Id);
            _context.SalesOrders.Update(salesOrder);
            await _context.SaveChangesAsync();

            decimal total = 0;
            decimal taxTotal = 0;

            // 2. Process Details----------------------------------------------------------------
            foreach (var detail in request.Details)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product == null)
                {
                    await transaction.RollbackAsync();
                    return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.ProductNotFound, ErrorType.Validation);
                }

                var Price = product.Price;

                var lineTotal = detail.Quantity * Price;
                var lineTax = lineTotal * (TaxRate / 100m);

                total += lineTotal + lineTax;
                taxTotal += lineTax;

                // Add SalesOrderDetail
                var salesOrderDetail = new SalesOrderDetail
                {
                    SalesOrderId = salesOrder.Id,
                    ProductId = detail.ProductId,
                    QuantityOrdered = detail.Quantity,
                    QuantityInvoiced = detail.Quantity,
                    Price = Price,
                    TaxRate = TaxRate
                };
                _context.SalesOrderDetails.Add(salesOrderDetail);

                var stockResult = await _stockService.DecreaseStockAsync(detail.ProductId, branchIdResult.Value, detail.Quantity);
                if (!stockResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return ToSalesOrderFailure(stockResult, SalesOrderError.StockUpdateFailed);
                }
            }

            salesOrder.Total = total;
            await _context.SaveChangesAsync();

            var billResult = await _billService.CreateAsync(new CreateBillRequestDto
            {
                BillType = request.BillType ?? BillTypeEnum.CONTADO,
                CustomerId = request.CustomerId,
                SalesOrderId = salesOrder.Id,
                Number = GenerateBillNumber(salesOrder.Id),
                Date = DateOnly.FromDateTime(request.Date ?? DateTime.UtcNow),
                Total = total,
                TaxTotal = taxTotal,
                BillState = BillStateEnum.Pending,
                IsCredit = request.IsCredit ?? false
            });

            if (!billResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return ToSalesOrderFailure(billResult, SalesOrderError.BillCreateFailed);
            }

            var billId = billResult.Value!.Bill.Id;

            foreach (var detail in request.Details)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product == null)
                {
                    await transaction.RollbackAsync();
                    return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.ProductNotFound, ErrorType.Validation);
                }

                var Price = product.Price;
                var billDetailResult = await _billDetailService.CreateAsync(new CreateBillDetailRequestDto
                {
                    BillId = billId,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = Price,
                    TaxRate = TaxRate
                });

                if (!billDetailResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return ToSalesOrderFailure(billDetailResult, SalesOrderError.BillDetailCreateFailed);
                }
            }

            // 4. Increase Account Balance----------------------------------------------------------------
            if (accountIdResult.Value > 0)
            {
                var account = await _context.Accounts.FindAsync(accountIdResult.Value);
                if (account != null)
                {
                    account.CurrentBalance += total;
                    account.AvailableBalance += total;
                    _context.Accounts.Update(account);

                    // After cange with Joshua🥵 services 
                    // 5. Create Bank Movement----------------------------------------------------------------
                    var bankMovement = new BankMovement
                    {
                        AccountId = account.Id,
                        MovementType = (BankMovementTypeEnum)movementType,
                        Date = DateTime.UtcNow,
                        Amount = total,
                        ReferenceNumber = $"SALE-{salesOrder.Id}"
                    };
                    _context.BankMovements.Add(bankMovement);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(salesOrder.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<SalesOrderWrapperDto>.Failure($"{SalesOrderError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<ListSalesOrdersWrapperDto>> GetAllAsync()
    {
        var salesOrders = await _context.SalesOrders
            .AsNoTracking()
            .ProjectTo<SalesOrderResponseDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(so => so.Date)
            .ToListAsync();

        return Result<ListSalesOrdersWrapperDto>.Success(new ListSalesOrdersWrapperDto { SalesOrders = salesOrders });
    }

    public async Task<Result<ListSalesOrdersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.SalesOrders.AsNoTracking();

        var totalElements = await query.CountAsync();

        var salesOrders = await query
            .OrderByDescending(so => so.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<SalesOrderResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListSalesOrdersWrapperDto>.Success(new ListSalesOrdersWrapperDto { SalesOrders = salesOrders, Pagination = _pagination });
    }

    public async Task<Result<SalesOrderWrapperDto>> GetByIdAsync(int id)
    {
        var salesOrder = await _context.SalesOrders
            .AsNoTracking()
            .Where(so => so.Id == id)
            .ProjectTo<SalesOrderResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (salesOrder == null)
            return Result<SalesOrderWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<SalesOrderWrapperDto>.Success(new SalesOrderWrapperDto { SalesOrder = salesOrder });
    }

    private static Result<SalesOrderWrapperDto> ToSalesOrderFailure<T>(Result<T> serviceResult, string fallbackMessage)
    {
        var message = string.IsNullOrWhiteSpace(serviceResult.ErrorMessage)
            ? fallbackMessage
            : serviceResult.ErrorMessage;

        if (serviceResult.Errors != null)
            return Result<SalesOrderWrapperDto>.Failure(message!, serviceResult.Errors, serviceResult.ErrorType);

        return Result<SalesOrderWrapperDto>.Failure(message!, serviceResult.ErrorType);
    }

    private static Result<SalesOrderWrapperDto> ToSalesOrderFailure(Result serviceResult, string fallbackMessage)
    {
        var message = string.IsNullOrWhiteSpace(serviceResult.ErrorMessage)
            ? fallbackMessage
            : serviceResult.ErrorMessage;

        if (serviceResult.Errors != null)
            return Result<SalesOrderWrapperDto>.Failure(message!, serviceResult.Errors, serviceResult.ErrorType);

        return Result<SalesOrderWrapperDto>.Failure(message!, serviceResult.ErrorType);
    }

    private static string GenerateSalesOrderNumber(int salesOrderId)
    {
        return $"SO-{salesOrderId:D6}";
    }

    private async Task<Result<int>> ResolveBranchIdAsync(int requestedBranchId)
    {
        if (requestedBranchId > 0)
        {
            var branchExists = await _context.Branches.AsNoTracking().AnyAsync(branch => branch.Id == requestedBranchId);
            if (branchExists)
                return Result<int>.Success(requestedBranchId);
        }

        var firstBranchId = await _context.Branches
            .AsNoTracking()
            .OrderBy(branch => branch.Id)
            .Select(branch => branch.Id)
            .FirstOrDefaultAsync();

        if (firstBranchId <= 0)
            return Result<int>.Failure(BranchError.BranchNotFound, ErrorType.NotFound);

        return Result<int>.Success(firstBranchId);
    }

    private async Task<Result<int>> ResolveAccountIdAsync(int requestedAccountId)
    {
        if (requestedAccountId > 0)
        {
            var accountExists = await _context.Accounts.AsNoTracking().AnyAsync(account => account.Id == requestedAccountId);
            if (accountExists)
                return Result<int>.Success(requestedAccountId);
        }

        var firstAccountId = await _context.Accounts
            .AsNoTracking()
            .OrderBy(account => account.Id)
            .Select(account => account.Id)
            .FirstOrDefaultAsync();

        return Result<int>.Success(firstAccountId);
    }

    private async Task<Result<int>> ResolveCustomerIdAsync(PosSaleCustomerRequestDto customer)
    {
        var ruc = customer.Ruc?.Trim();
        if (string.IsNullOrWhiteSpace(ruc))
            return Result<int>.Failure("customer.ruc es obligatorio.", ErrorType.Validation);

        var existingCustomer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Ruc == ruc);

        if (existingCustomer != null)
            return Result<int>.Success(existingCustomer.Id);

        var name = customer.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result<int>.Failure("customer.name es obligatorio cuando el cliente no existe.", ErrorType.Validation);

        var createdCustomerResult = await _customerService.CreateAsync(new CreateCustomerRequestDto
        {
            Name = name,
            Ruc = ruc
        });

        if (!createdCustomerResult.IsSuccess)
        {
            if (createdCustomerResult.Errors != null)
                return Result<int>.Failure(createdCustomerResult.ErrorMessage!, createdCustomerResult.Errors, createdCustomerResult.ErrorType);

            return Result<int>.Failure(createdCustomerResult.ErrorMessage ?? SalesOrderError.CustomerNotFound, createdCustomerResult.ErrorType);
        }

        return Result<int>.Success(createdCustomerResult.Value!.Customer.Id);
    }

    private async Task<Result<int>> ResolveProductIdAsync(PosSaleProductRequestDto product)
    {
        if (product.Quantity <= 0)
            return Result<int>.Failure("Cada producto debe tener quantity > 0.", ErrorType.Validation);

        if (product.ProductId.HasValue)
            return Result<int>.Success(product.ProductId.Value);

        if (string.IsNullOrWhiteSpace(product.Barcode))
            return Result<int>.Failure("Cada producto debe tener productId o barcode.", ErrorType.Validation);

        var barcode = product.Barcode.Trim();
        var productEntity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (productEntity == null)
            return Result<int>.Failure($"No se encontro producto con barcode {barcode}.", ErrorType.Validation);

        return Result<int>.Success(productEntity.Id);
    }

    private static string GenerateBillNumber(int salesOrderId)
    {
        return $"001-001-{salesOrderId:D6}";
    }
}
