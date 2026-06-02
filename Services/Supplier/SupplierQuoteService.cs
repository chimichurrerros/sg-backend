using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.SupplierQuote;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.SupplierQuote;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class SupplierQuoteService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListSupplierQuotesWrapperDto>> GetAllAsync()
    {
        var quotes = await _context.SupplierQuotes
            .AsNoTracking()
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.RequestForQuotation)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(q => q.Id)
            .ToListAsync();

        var dtos = _mapper.Map<List<SupplierQuoteResponseDto>>(quotes);

        return Result<ListSupplierQuotesWrapperDto>.Success(new ListSupplierQuotesWrapperDto
        {
            SupplierQuotes = dtos
        });
    }

    public async Task<Result<ListSupplierQuotesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.SupplierQuotes
            .AsNoTracking()
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.RequestForQuotation)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product);

        var total = await query.CountAsync();

        var quotes = await query
            .OrderByDescending(q => q.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<SupplierQuoteResponseDto>>(quotes);

        var paginationData = new Pagination(pagination.Page, pagination.PageSize, total);

        return Result<ListSupplierQuotesWrapperDto>.Success(new ListSupplierQuotesWrapperDto
        {
            SupplierQuotes = dtos,
            Pagination = paginationData
        });
    }

    public async Task<Result<SupplierQuoteWrapperDto>> GetByIdAsync(int id)
    {
        var quote = await _context.SupplierQuotes
            .AsNoTracking()
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.RequestForQuotation)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<SupplierQuoteWrapperDto>.Failure(SupplierQuoteError.SupplierQuoteNotFound, ErrorType.NotFound);

        return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(quote));
    }

    public async Task<Result<SupplierQuoteWrapperDto>> CreateAsync(CreateSupplierQuoteRequestDto request)
    {
        var validation = await ValidateCreateRequestAsync(request);
        if (!validation.IsSuccess)
            return Result<SupplierQuoteWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var quote = _mapper.Map<SupplierQuote>(request);
            quote.Date = DateTime.UtcNow;
            quote.Total = CalculateTotal(request.Details);

            _context.SupplierQuotes.Add(quote);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var created = await _context.SupplierQuotes
                .AsNoTracking()
                .Include(q => q.Supplier)
                .Include(q => q.PurchaseOrders)
                .Include(q => q.PurchaseRequest)
                .Include(q => q.RequestForQuotation)
                .Include(q => q.SupplierQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == quote.Id);

            return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(created));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<SupplierQuoteWrapperDto>> UpdateAsync(int id, UpdateSupplierQuoteRequestDto request)
    {
        var quote = await _context.SupplierQuotes
            .Include(q => q.SupplierQuoteDetails)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<SupplierQuoteWrapperDto>.Failure(SupplierQuoteError.SupplierQuoteNotFound, ErrorType.NotFound);

        var validation = await ValidateUpdateRequestAsync(request, id, quote.SupplierId, quote.PurchaseRequestId, quote.RequestForQuotationId);
        if (!validation.IsSuccess)
            return Result<SupplierQuoteWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.SupplierId.HasValue)
                quote.SupplierId = request.SupplierId.Value;

            if (request.PurchaseRequestId.HasValue)
                quote.PurchaseRequestId = request.PurchaseRequestId.Value;

            if (request.RequestForQuotationId.HasValue)
                quote.RequestForQuotationId = request.RequestForQuotationId.Value;

            if (request.Details != null)
            {
                _context.SupplierQuoteDetails.RemoveRange(quote.SupplierQuoteDetails);

                quote.SupplierQuoteDetails = request.Details
                    .Select(d => new SupplierQuoteDetail
                    {
                        ProductId = d.ProductId,
                        QuantityAvailable = d.QuantityAvailable,
                        Price = d.Price
                    })
                    .ToList();

                quote.Total = CalculateTotal(request.Details);
            }

            if (request.State.HasValue && Enum.IsDefined(typeof(SupplierQuoteStateEnum), request.State.Value))
            {
                quote.State = (SupplierQuoteStateEnum)request.State.Value;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updated = await _context.SupplierQuotes
                .AsNoTracking()
                .Include(q => q.Supplier)
                .Include(q => q.PurchaseOrders)
                .Include(q => q.PurchaseRequest)
                .Include(q => q.RequestForQuotation)
                .Include(q => q.SupplierQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(updated));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static decimal CalculateTotal(List<SupplierQuoteDetailRequestDto> details)
    {
        return details.Sum(d => d.QuantityAvailable * d.Price);
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateSupplierQuoteRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.SupplierId <= 0)
            errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierIdRequired];

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestIdRequired];

        if (request.RequestForQuotationId <= 0)
            errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationIdRequired];

        ValidateDetails(request.Details, errors, nameof(request.Details));

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
        if (!supplierExists)
            errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierNotFound];

        var prExists = await _context.PurchaseRequests.AnyAsync(p => p.Id == request.PurchaseRequestId);
        if (!prExists)
            errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestNotFound];

        var rfq = await _context.RequestForQuotations
            .FirstOrDefaultAsync(r => r.Id == request.RequestForQuotationId);
        if (rfq == null)
        {
            errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationNotFound];
        }
        else
        {
            if (rfq.SupplierId != request.SupplierId || rfq.PurchaseRequestId != request.PurchaseRequestId)
                errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationMismatch];
            else
            {
                var existingQuote = await _context.SupplierQuotes
                    .AnyAsync(q => q.RequestForQuotationId == request.RequestForQuotationId);
                if (existingQuote)
                    errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.DuplicateQuote];
            }
        }

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        var productsValidation = await ValidateProductsBelongToRfqAsync(request.RequestForQuotationId, request.Details.Select(d => d.ProductId).ToList());
        if (!productsValidation.IsSuccess)
            errors[nameof(request.Details)] = [SupplierQuoteError.InvalidProducts];

        var quantitiesValidation = await ValidateQuantitiesAgainstRfqAsync(request.RequestForQuotationId, request.Details);
        if (!quantitiesValidation.IsSuccess)
            errors[nameof(request.Details)] = [SupplierQuoteError.QuantityExceedsRequested];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdateSupplierQuoteRequestDto request, int quoteId, int currentSupplierId, int currentPurchaseRequestId, int currentRequestForQuotationId)
    {
        var errors = new Dictionary<string, string[]>();

        var supplierId = currentSupplierId;
        var purchaseRequestId = currentPurchaseRequestId;
        var requestForQuotationId = currentRequestForQuotationId;

        if (request.SupplierId.HasValue)
        {
            if (request.SupplierId <= 0)
                errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierIdRequired];
            else if (request.SupplierId != currentSupplierId)
            {
                var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
                if (!supplierExists)
                    errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierNotFound];
                else
                    supplierId = request.SupplierId.Value;
            }
        }

        if (request.PurchaseRequestId.HasValue)
        {
            if (request.PurchaseRequestId <= 0)
                errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestIdRequired];
            else if (request.PurchaseRequestId != currentPurchaseRequestId)
            {
                var prExists = await _context.PurchaseRequests.AnyAsync(p => p.Id == request.PurchaseRequestId);
                if (!prExists)
                    errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestNotFound];
                else
                    purchaseRequestId = request.PurchaseRequestId.Value;
            }
        }

        if (request.RequestForQuotationId.HasValue)
        {
            if (request.RequestForQuotationId <= 0)
                errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationIdRequired];
            else if (request.RequestForQuotationId != currentRequestForQuotationId)
            {
                var rfq = await _context.RequestForQuotations
                    .FirstOrDefaultAsync(r => r.Id == request.RequestForQuotationId);

                if (rfq == null)
                {
                    errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationNotFound];
                }
                else
                {
                    if (rfq.SupplierId != supplierId || rfq.PurchaseRequestId != purchaseRequestId)
                        errors[nameof(request.RequestForQuotationId)] = [SupplierQuoteError.RequestForQuotationMismatch];
                    else
                        requestForQuotationId = request.RequestForQuotationId.Value;
                }
            }
        }

        if (request.Details != null)
        {
            var hasAssociatedPurchaseOrders = await _context.SupplierQuoteDetails
                .Where(d => d.SupplierQuoteId == quoteId)
                .SelectMany(d => d.PurchaseOrderDetails)
                .AnyAsync();

            if (hasAssociatedPurchaseOrders)
            {
                errors[nameof(request.Details)] = ["No se pueden actualizar los detalles de una cotizacion que ya tiene ordenes de compra asociadas."];
            }
            else
            {
                ValidateDetails(request.Details, errors, nameof(request.Details));

                if (!errors.ContainsKey(nameof(request.Details)))
                {
                    var productsValidation = await ValidateProductsBelongToRfqAsync(requestForQuotationId, request.Details.Select(d => d.ProductId).ToList());
                    if (!productsValidation.IsSuccess)
                        errors[nameof(request.Details)] = [SupplierQuoteError.InvalidProducts];

                    var quantitiesValidation = await ValidateQuantitiesAgainstRfqAsync(requestForQuotationId, request.Details);
                    if (!quantitiesValidation.IsSuccess)
                        errors[nameof(request.Details)] = [SupplierQuoteError.QuantityExceedsRequested];
                }
            }
        }

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    private static void ValidateDetails(List<SupplierQuoteDetailRequestDto> details, Dictionary<string, string[]> errors, string fieldName)
    {
        if (details == null || details.Count == 0)
        {
            errors[fieldName] = [SupplierQuoteError.DetailsRequired];
            return;
        }

        if (details.Any(d => d.QuantityAvailable < 0))
            errors[$"{fieldName}.QuantityAvailable"] = [SupplierQuoteError.InvalidDetailQuantity];

        if (details.Any(d => d.Price < 0))
            errors[$"{fieldName}.Price"] = [SupplierQuoteError.InvalidDetailPrice];
    }

    private async Task<Result> ValidateProductsBelongToRfqAsync(int requestForQuotationId, List<int> productIds)
    {
        var distinct = productIds.Distinct().ToList();
        if (distinct.Count == 0)
            return Result.Failure(SupplierQuoteError.InvalidProducts, ErrorType.Validation);

        var count = await _context.RequestForQuotationDetails
            .CountAsync(d => d.RequestForQuotationId == requestForQuotationId && distinct.Contains(d.ProductId));

        if (count != distinct.Count)
            return Result.Failure(SupplierQuoteError.InvalidProducts, ErrorType.Validation);

        return Result.Success();
    }

    private async Task<Result> ValidateQuantitiesAgainstRfqAsync(int requestForQuotationId, List<SupplierQuoteDetailRequestDto> details)
    {
        var rfqDetails = await _context.RequestForQuotationDetails
            .Where(d => d.RequestForQuotationId == requestForQuotationId)
            .ToListAsync();

        var rfqByProduct = rfqDetails.ToDictionary(d => d.ProductId, d => d.QuantityRequested);

        foreach (var detail in details)
        {
            if (rfqByProduct.TryGetValue(detail.ProductId, out var requested) && detail.QuantityAvailable > requested)
                return Result.Failure(SupplierQuoteError.QuantityExceedsRequested, ErrorType.Validation);
        }

        return Result.Success();
    }
}
