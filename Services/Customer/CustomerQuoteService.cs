using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Customer;
using BackEnd.DTOs.Requests.CustomerQuote;
using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class CustomerQuoteService(AppDbContext context, CustomerService customerService, SalesOrderService salesOrderService, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly CustomerService _customerService = customerService;
    private readonly SalesOrderService _salesOrderService = salesOrderService;
    private readonly IMapper _mapper = mapper;
    private const int QuoteValidityDays = 10;

    public async Task<Result<ListCustomerQuotesWrapperDto>> GetAllAsync()
    {
        await ExpireQuotesIfNeededAsync();

        var quotes = await _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.Branch)
            .Include(q => q.SalesOrders)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(q => q.Id)
            .ToListAsync();

        var quoteDtos = _mapper.Map<List<CustomerQuoteResponseDto>>(quotes);

        return Result<ListCustomerQuotesWrapperDto>.Success(new ListCustomerQuotesWrapperDto
        {
            CustomerQuotes = quoteDtos
        });
    }

    public async Task<Result<ListCustomerQuotesWrapperDto>> GetListAsync(CustomerQuoteQueryDto queryDto)
    {
        await ExpireQuotesIfNeededAsync();

        IQueryable<CustomerQuote> quotesQuery = _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.Branch)
            .Include(q => q.SalesOrders)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product);

        if (queryDto.Id.HasValue)
            quotesQuery = quotesQuery.Where(q => q.Id == queryDto.Id.Value);

        if (queryDto.Date.HasValue)
            quotesQuery = quotesQuery.Where(q => q.Date.Date == queryDto.Date.Value.Date);

        if (queryDto.ExpirationDate.HasValue)
        {
            var calendarDays = DateTimeUtils.WorkingDaysToCalendarDays(QuoteValidityDays);
            var targetCreationDate = queryDto.ExpirationDate.Value.AddDays(-calendarDays);
            quotesQuery = quotesQuery.Where(q => q.Date.Date == targetCreationDate.Date);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerName))
            quotesQuery = quotesQuery.Where(q => q.Customer != null && q.Customer.Name.ToLower().Contains(queryDto.CustomerName.ToLower()));

        if (queryDto.CustomerId.HasValue)
            quotesQuery = quotesQuery.Where(q => q.CustomerId == queryDto.CustomerId.Value);

        var totalElements = await quotesQuery.CountAsync();

        var quotes = await quotesQuery
            .OrderByDescending(q => q.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        var quoteDtos = _mapper.Map<List<CustomerQuoteResponseDto>>(quotes);
        var paginationData = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListCustomerQuotesWrapperDto>.Success(new ListCustomerQuotesWrapperDto
        {
            CustomerQuotes = quoteDtos,
            Pagination = paginationData
        });
    }

    public async Task<Result<CustomerQuoteWrapperDto>> GetByIdAsync(int id)
    {
        await ExpireQuotesIfNeededAsync();

        var quote = await _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.Branch)
            .Include(q => q.SalesOrders)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(quote));
    }

    public async Task<Result<CustomerQuoteWrapperDto>> CreateAsync(CreateCustomerQuoteRequestDto request, int userId)
    {
        if (request.Products.Count == 0)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.DetailsRequired, ErrorType.Validation);

        var customerIdResult = await ResolveCustomerIdAsync(request.Customer!);
        if (!customerIdResult.IsSuccess)
            return Result<CustomerQuoteWrapperDto>.Failure(customerIdResult.ErrorMessage!, customerIdResult.ErrorType);

        var branchId = request.Sale.BranchId ?? request.Sale.CashierNumber ?? 0;
        if (branchId <= 0)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.BranchRequired, ErrorType.Validation);

        await ExpireQuotesIfNeededAsync(customerIdResult.Value);

        var hasOpenQuote = await _context.CustomerQuotes.AnyAsync(q =>
            q.CustomerId == customerIdResult.Value && q.Status == CustomerQuote.QuoteStatus.Open);

        if (hasOpenQuote)
        {
            var errors = new Dictionary<string, string[]>
            {
                [nameof(request.Customer)] = [CustomerQuoteError.ExistingOpenQuote]
            };

            return Result<CustomerQuoteWrapperDto>.Failure(
                CustomerQuoteError.ExistingOpenQuote,
                errors,
                ErrorType.Conflict);
        }

        var details = new List<CustomerQuoteDetail>();
        foreach (var product in request.Products)
        {
            var productIdResult = await ResolveProductIdAsync(product);
            if (!productIdResult.IsSuccess)
                return Result<CustomerQuoteWrapperDto>.Failure(productIdResult.ErrorMessage!, productIdResult.ErrorType);

            details.Add(new CustomerQuoteDetail
            {
                ProductId = productIdResult.Value,
                Quantity = product.Quantity,
                Price = product.Price
            });
        }

        var isCredit = request.Pay.Condition == PosSaleCondition.Credit;
        var billType = request.Sale.Bill ?? (isCredit ? BillTypeEnum.CREDITO : BillTypeEnum.CONTADO);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
         {
            var quote = new CustomerQuote
            {
                CustomerId = customerIdResult.Value,
                UserId = userId,
                BranchId = branchId,
                Date = request.Sale.Date ?? DateTime.UtcNow,
                Total = details.Sum(d => d.Quantity * d.Price),
                ImportValue = request.Totals.ImportValue,
                PaymentMethod = (PaymentMethodEnum)request.Pay.Method,
                SaleCondition = (SaleConditionEnum)request.Pay.Condition,
                BillType = billType,
                AccountId = request.Sale.AccountId,
                MovementType = request.Sale.MovementType,
                CashierNumber = request.Sale.CashierNumber,
                Status = CustomerQuote.QuoteStatus.Open,
                CustomerQuoteDetails = details
            };

            _context.CustomerQuotes.Add(quote);
            await _context.SaveChangesAsync();

            quote.Number = $"COT-{quote.Id:D6}";
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdQuote = await _context.CustomerQuotes
                .AsNoTracking()
                .Include(q => q.Customer)
                .Include(q => q.User)
                .Include(q => q.Branch)
                .Include(q => q.SalesOrders)
                .Include(q => q.CustomerQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == quote.Id);

            return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(createdQuote));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<CustomerQuoteWrapperDto>> UpdateAsync(int id, UpdateCustomerQuoteRequestDto request, int userId)
    {
        var quote = await _context.CustomerQuotes
            .Include(q => q.CustomerQuoteDetails)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        await ExpireQuoteIfNeededAsync(quote);

        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
        {
            var errors = new Dictionary<string, string[]>
            {
                [nameof(id)] = [CustomerQuoteError.QuoteExpired]
            };

            return Result<CustomerQuoteWrapperDto>.Failure(
                CustomerQuoteError.QuoteExpired,
                errors,
                ErrorType.Conflict);
        }

        if (request.Products.Count == 0)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.DetailsRequired, ErrorType.Validation);

        var customerIdResult = await ResolveCustomerIdAsync(request.Customer!);
        if (!customerIdResult.IsSuccess)
            return Result<CustomerQuoteWrapperDto>.Failure(customerIdResult.ErrorMessage!, customerIdResult.ErrorType);

        var branchId = request.Sale.BranchId ?? request.Sale.CashierNumber ?? quote.BranchId;
        if (branchId <= 0)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.BranchRequired, ErrorType.Validation);

        var details = new List<CustomerQuoteDetail>();
        foreach (var product in request.Products)
        {
            var productIdResult = await ResolveProductIdAsync(product);
            if (!productIdResult.IsSuccess)
                return Result<CustomerQuoteWrapperDto>.Failure(productIdResult.ErrorMessage!, productIdResult.ErrorType);

            details.Add(new CustomerQuoteDetail
            {
                ProductId = productIdResult.Value,
                Quantity = product.Quantity,
                Price = product.Price
            });
        }

        var isCredit = request.Pay.Condition == PosSaleCondition.Credit;
        var billType = request.Sale.Bill ?? (isCredit ? BillTypeEnum.CREDITO : BillTypeEnum.CONTADO);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
         {
            quote.CustomerId = customerIdResult.Value;
            quote.UserId = userId;
            quote.BranchId = branchId;
            quote.Date = request.Sale.Date ?? quote.Date;
            quote.Total = details.Sum(d => d.Quantity * d.Price);
            quote.ImportValue = request.Totals.ImportValue;
            quote.PaymentMethod = (PaymentMethodEnum)request.Pay.Method;
            quote.SaleCondition = (SaleConditionEnum)request.Pay.Condition;
            quote.BillType = billType;
            quote.AccountId = request.Sale.AccountId;
            quote.MovementType = request.Sale.MovementType;
            quote.CashierNumber = request.Sale.CashierNumber;

            _context.CustomerQuoteDetails.RemoveRange(quote.CustomerQuoteDetails);
            quote.CustomerQuoteDetails = details;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updatedQuote = await _context.CustomerQuotes
                .AsNoTracking()
                .Include(q => q.Customer)
                .Include(q => q.User)
                .Include(q => q.Branch)
                .Include(q => q.SalesOrders)
                .Include(q => q.CustomerQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(updatedQuote));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<SalesOrderWrapperDto>> SellFromQuoteAsync(int quoteId, int userId)
    {
        var quote = await _context.CustomerQuotes
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
            return Result<SalesOrderWrapperDto>.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        await ExpireQuoteIfNeededAsync(quote);

        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
            return Result<SalesOrderWrapperDto>.Failure(CustomerQuoteError.QuoteExpired, ErrorType.Conflict);

        if (quote.Status == CustomerQuote.QuoteStatus.Closed)
            return Result<SalesOrderWrapperDto>.Failure(CustomerQuoteError.QuoteAlreadySold, ErrorType.Conflict);

        // --- MODIFICADO: VALIDACIÓN ESTRICTA DE STOCK ANTES DE EMITIR LA VENTA ---
        foreach (var detail in quote.CustomerQuoteDetails)
        {
            // Buscamos el registro de stock correspondiente al producto y a la sucursal del presupuesto
            var stock = await _context.Set<Stock>()
                .FirstOrDefaultAsync(s => s.ProductId == detail.ProductId && s.BranchId == quote.BranchId);

            if (stock == null || stock.Quantity < detail.Quantity)
            {
                var productName = detail.Product?.Name ?? $"ID {detail.ProductId}";
                var availableStock = stock?.Quantity ?? 0;

                return Result<SalesOrderWrapperDto>.Failure(
                    $"Stock insuficiente para procesar el presupuesto. Producto: '{productName}'. Requerido: {detail.Quantity}, Disponible: {availableStock}.", 
                    ErrorType.Validation);
            }
        }
        // ------------------------------------------------------------------------

        var details = quote.CustomerQuoteDetails.Select(d => new CreateSalesOrderDetailRequestDto
        {
            ProductId = d.ProductId,
            Quantity = d.Quantity,
            Price = d.Price
        }).ToList();

        var isCredit = quote.SaleCondition == SaleConditionEnum.Credit;
        var movementType = quote.MovementType ?? (int)BankMovementTypeEnum.Credit;

        var request = new CreateSalesOrderRequestDto
        {
            CustomerId = quote.CustomerId,
            SalesOrderState = SalesOrderStateEnum.Confirmed,
            Date = DateTime.UtcNow,
            BillType = quote.BillType,
            IsCredit = isCredit,
            PaymentMethod = quote.PaymentMethod,
            SaleCondition = quote.SaleCondition,
            AccountId = quote.AccountId ?? 0,
            MovementType = movementType,
            BranchId = quote.BranchId,
            ImportValue = quote.ImportValue,
            CustomerQuoteId = quote.Id,
            Details = details
        };

        var result = await _salesOrderService.CreateAsync(request, userId);

        if (!result.IsSuccess)
            return result;

        quote.Status = CustomerQuote.QuoteStatus.Closed;
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<Result> CancelAsync(int id)
    {
        var quote = await _context.CustomerQuotes
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        await ExpireQuoteIfNeededAsync(quote);

        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
            return Result.Failure(CustomerQuoteError.QuoteExpired, ErrorType.Conflict);

        if (quote.Status == CustomerQuote.QuoteStatus.Closed)
            return Result.Failure(CustomerQuoteError.QuoteAlreadySold, ErrorType.Conflict);

        quote.Status = CustomerQuote.QuoteStatus.Cancelled;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private async Task<Result<int>> ResolveCustomerIdAsync(CustomerQuoteCustomerRequestDto customer)
    {
        var ruc = customer.Ruc?.Trim();

        var existingCustomer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Ruc == ruc);

        if (existingCustomer != null)
            return Result<int>.Success(existingCustomer.Id);

        var name = customer.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result<int>.Failure(CustomerError.NameRequiredForNewCustomer, ErrorType.Validation);

        var createdCustomerResult = await _customerService.CreateAsync(new CreateCustomerRequestDto
        {
            Name = name,
            Ruc = ruc ?? string.Empty,
            BirthDate = customer.BirthDate,
            Email = customer.Email
        });

        if (!createdCustomerResult.IsSuccess)
        {
            if (createdCustomerResult.Errors != null)
                return Result<int>.Failure(createdCustomerResult.ErrorMessage!, createdCustomerResult.Errors, createdCustomerResult.ErrorType);

            return Result<int>.Failure(createdCustomerResult.ErrorMessage ?? CustomerQuoteError.CustomerNotFound, createdCustomerResult.ErrorType);
        }

        return Result<int>.Success(createdCustomerResult.Value!.Customer.Id);
    }

    private async Task<Result<int>> ResolveProductIdAsync(CustomerQuoteProductRequestDto product)
    {
        if (product.Quantity <= 0)
            return Result<int>.Failure(CustomerQuoteError.ProductQuantityRequired, ErrorType.Validation);

        if (product.ProductId.HasValue)
            return Result<int>.Success(product.ProductId.Value);

        if (string.IsNullOrWhiteSpace(product.Barcode))
            return Result<int>.Failure(CustomerQuoteError.ProductIdOrBarcodeRequired, ErrorType.Validation);

        var barcode = product.Barcode.Trim();
        var productEntity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (productEntity == null)
            return Result<int>.Failure(string.Format(CustomerQuoteError.ProductNotFoundWithBarcode, barcode), ErrorType.Validation);

        return Result<int>.Success(productEntity.Id);
    }

    private async Task ExpireQuotesIfNeededAsync(int? customerId = null)
    {
        var utcNow = DateTime.UtcNow;
        var openQuotesQuery = _context.CustomerQuotes
            .Where(q => q.Status == CustomerQuote.QuoteStatus.Open);

        if (customerId.HasValue)
            openQuotesQuery = openQuotesQuery.Where(q => q.CustomerId == customerId.Value);

        var calendarDays = DateTimeUtils.WorkingDaysToCalendarDays(QuoteValidityDays);

        var quotesToExpire = await openQuotesQuery
            .Where(q => q.Date.AddDays(calendarDays) < utcNow)
            .ToListAsync();

        quotesToExpire = quotesToExpire
            .Where(q => DateTimeUtils.AddWorkingDays(q.Date, QuoteValidityDays) < utcNow)
            .ToList();

        if (quotesToExpire.Count == 0)
            return;

        foreach (var quote in quotesToExpire)
            quote.Status = CustomerQuote.QuoteStatus.Expired;

        await _context.SaveChangesAsync();
    }

    private async Task ExpireQuoteIfNeededAsync(CustomerQuote quote)
    {
        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
            return;

        if (DateTimeUtils.AddWorkingDays(quote.Date, QuoteValidityDays) >= DateTime.UtcNow)
            return;

        quote.Status = CustomerQuote.QuoteStatus.Expired;
        await _context.SaveChangesAsync();
    }
}