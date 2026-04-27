using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Bill;
using BackEnd.DTOs.Requests.BillDetail;
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

    public async Task<Result<SalesOrderWrapperDto>> CreateAsync(CreateSalesOrderRequestDto request, int userId)
    {
        if (request.Details.Count == 0)
            return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.DetailsRequired, ErrorType.Validation);

        var customerResult = await _customerService.GetByIdAsync(request.CustomerId);
        if (!customerResult.IsSuccess)
            return ToSalesOrderFailure(customerResult, SalesOrderError.CustomerNotFound);

        var branchResult = await _branchService.GetByIdAsync(request.BranchId);
        if (!branchResult.IsSuccess)
            return ToSalesOrderFailure(branchResult, BranchError.BranchNotFound);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Create SalesOrder-------------------------------------------------------------
            var salesOrder = new SalesOrder
            {
                CustomerId = request.CustomerId,
                UserId = userId,
                Number = request.Number,
                Date = DateTime.UtcNow,
                SalesOrderState = request.SalesOrderState,
                Total = 0 // Will compute
            };

            _context.SalesOrders.Add(salesOrder);
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
                    return Result<SalesOrderWrapperDto>.Failure($"Product with ID {detail.ProductId} not found", ErrorType.Validation);
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

                var stockResult = await _stockService.DecreaseStockAsync(detail.ProductId, request.BranchId, detail.Quantity);
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
                BillType = BillTypeEnum.CONTADO,
                CustomerId = request.CustomerId,
                SalesOrderId = salesOrder.Id,
                Number = request.BillNumber,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Total = total,
                TaxTotal = taxTotal,
                BillState = request.BillState,
                IsCredit = false
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
                    return Result<SalesOrderWrapperDto>.Failure($"Product with ID {detail.ProductId} not found", ErrorType.Validation);
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
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null)
            {
                await transaction.RollbackAsync();
                return Result<SalesOrderWrapperDto>.Failure(SalesOrderError.AccountNotFound, ErrorType.NotFound);
            }

            account.CurrentBalance += total;
            account.AvailableBalance += total;
            _context.Accounts.Update(account);

            // After cange with Joshua🥵 services 
            // 5. Create Bank Movement----------------------------------------------------------------
            var bankMovement = new BankMovement
            {
                AccountId = request.AccountId,
                MovementType = (BankMovementTypeEnum)request.MovementType,
                Date = DateTime.UtcNow,
                Amount = total,
                ReferenceNumber = $"SALE-{salesOrder.Id}"
            };
            _context.BankMovements.Add(bankMovement);

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
}
