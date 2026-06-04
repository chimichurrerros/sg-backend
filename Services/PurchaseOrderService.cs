using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PurchaseOrderService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListPurchaseOrdersWrapperDto>> GetAllAsync()
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders)
        });
    }

    public async Task<Result<ListPurchaseOrdersWrapperDto>> GetListAsync(PurchaseOrderQueryDto query)
    {
        var rQuery = _context.PurchaseOrders.AsNoTracking();

        if (query.PurchaseRequestId.HasValue)
            rQuery = rQuery.Where(o => o.PurchaseRequestId == query.PurchaseRequestId.Value);

        if (query.State.HasValue)
            rQuery = rQuery.Where(o => (int)o.State == query.State.Value);

        if (query.Date.HasValue)
            rQuery = rQuery.Where(o => DateOnly.FromDateTime(o.Date) == query.Date.Value);

        if (query.StartDate.HasValue)
            rQuery = rQuery.Where(o => DateOnly.FromDateTime(o.Date) >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            rQuery = rQuery.Where(o => DateOnly.FromDateTime(o.Date) <= query.EndDate.Value);

        if (query.MinTotal.HasValue)
            rQuery = rQuery.Where(o => o.Total >= query.MinTotal.Value);

        if (query.MaxTotal.HasValue)
            rQuery = rQuery.Where(o => o.Total <= query.MaxTotal.Value);

        var total = await rQuery.CountAsync();

        var orders = await rQuery
            .OrderByDescending(o => o.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders),
            Pagination = new Pagination(query.Page, query.PageSize, total)
        });
    }

    public async Task<Result<PurchaseOrderWrapperDto>> GetByIdAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.PurchaseOrdersForSupplier)
                .ThenInclude(pos => pos.Supplier)
            .Include(o => o.PurchaseOrdersForSupplier)
                .ThenInclude(pos => pos.PurchaseOrderDetails)
                    .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(order));
    }

    public async Task<Result<PurchaseOrderDraftWrapperDto>> GetDraftByPurchaseRequestIdAsync(int purchaseRequestId)
    {
        var draft = await BuildDraftDataAsync(purchaseRequestId);
        if (!draft.IsSuccess)
            return Result<PurchaseOrderDraftWrapperDto>.Failure(draft.ErrorMessage!, draft.Errors!, draft.ErrorType);

        return Result<PurchaseOrderDraftWrapperDto>.Success(new PurchaseOrderDraftWrapperDto
        {
            PurchaseOrder = BuildDraftResponse(draft.Value!)
        });
    }

    public async Task<Result<PurchaseOrderWrapperDto>> CreateAsync(CreatePurchaseOrderRequestDto request)
    {
        var validation = await ValidateCreateRequestAsync(request);
        if (!validation.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        var draftResult = await BuildDraftDataAsync(request.PurchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        var resolved = ApplyOverrides(draftResult.Value!, request.Details);
        if (!resolved.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(resolved.ErrorMessage!, resolved.Errors!, resolved.ErrorType);

        var resolvedLines = resolved.Value!;

        var groupedBySupplier = resolvedLines
            .GroupBy(d => d.SupplierId ?? 0)
            .ToList();

        if (groupedBySupplier.Count == 0)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.DetailsRequired, ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var mainOrder = new PurchaseOrder
            {
                PurchaseRequestId = request.PurchaseRequestId,
                Number = string.Empty,
                Date = DateTime.UtcNow,
                Total = resolvedLines.Sum(d => d.Price * d.QuantityOrdered),
                State = PurchaseOrderStateEnum.Pending
            };

            _context.PurchaseOrders.Add(mainOrder);
            await _context.SaveChangesAsync();

            mainOrder.Number = GeneratePurchaseOrderNumber(mainOrder.Id);
            _context.PurchaseOrders.Update(mainOrder);
            await _context.SaveChangesAsync();

            var sequence = 1;
            foreach (var group in groupedBySupplier)
            {
                var supplierId = group.Key;
                var lines = group.ToList();

                var allSameQuote = lines.Select(l => l.SupplierQuoteId).Distinct().Count() == 1;
                var supplierQuoteId = allSameQuote ? lines.First().SupplierQuoteId : null;

                var childOrder = new PurchaseOrderForSupplier
                {
                    PurchaseOrderId = mainOrder.Id,
                    SupplierId = supplierId,
                    SupplierQuoteId = supplierQuoteId,
                    Number = GenerateChildPurchaseOrderNumber(mainOrder.Number, sequence),
                    Date = DateTime.UtcNow,
                    Total = lines.Sum(l => l.Price * l.QuantityOrdered),
                    State = PurchaseOrderForSupplierStateEnum.Pending
                };

                _context.PurchaseOrdersForSupplier.Add(childOrder);
                await _context.SaveChangesAsync();

                foreach (var line in lines)
                {
                    _context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                    {
                        PurchaseOrderForSupplierId = childOrder.Id,
                        ProductId = line.ProductId,
                        SupplierQuoteDetailId = line.SupplierQuoteDetailId,
                        QuantityOrdered = line.QuantityOrdered,
                        QuantityReceived = 0,
                        Price = line.Price,
                        TaxRate = line.TaxRate
                    });
                }

                sequence++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var created = await _context.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == mainOrder.Id);

            return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(created));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> CancelMainOrderAsync(int purchaseOrderId)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.PurchaseOrdersForSupplier)
            .FirstOrDefaultAsync(o => o.Id == purchaseOrderId);

        if (order == null)
            return Result<bool>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        order.State = PurchaseOrderStateEnum.Cancelled;

        foreach (var child in order.PurchaseOrdersForSupplier)
        {
            child.State = PurchaseOrderForSupplierStateEnum.Cancelled;
        }

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private async Task<Result<ResolvedPurchaseOrderDraft>> BuildDraftDataAsync(int purchaseRequestId)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .AsNoTracking()
            .Include(pr => pr.PurchaseRequestDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

        if (purchaseRequest == null)
            return Result<ResolvedPurchaseOrderDraft>.Failure(PurchaseOrderError.PurchaseRequestNotFound, ErrorType.NotFound);

        var quoteDetails = await _context.SupplierQuoteDetails
            .AsNoTracking()
            .Include(qd => qd.Product)
            .Include(qd => qd.SupplierQuote)
                .ThenInclude(sq => sq.Supplier)
            .Where(qd => qd.SupplierQuote.PurchaseRequestId == purchaseRequestId)
            .ToListAsync();

        var resolvedLines = new List<ResolvedPurchaseOrderLine>();
        var errors = new Dictionary<string, string[]>();

        foreach (var requestDetail in purchaseRequest.PurchaseRequestDetails)
        {
            var candidates = quoteDetails
                .Where(qd => qd.ProductId == requestDetail.ProductId)
                .OrderBy(qd => qd.Price)
                .ThenBy(qd => qd.SupplierQuoteId)
                .ToList();

            if (candidates.Count == 0)
            {
                errors[$"PurchaseRequestDetails[{requestDetail.ProductId}]"] = [PurchaseOrderError.InvalidProducts];
                continue;
            }

            var bestPricedOption = candidates.First();

            resolvedLines.Add(new ResolvedPurchaseOrderLine
            {
                ProductId = requestDetail.ProductId,
                QuantityOrdered = requestDetail.QuantityRequested,
                SupplierQuoteDetailId = bestPricedOption.Id,
                SupplierQuoteId = bestPricedOption.SupplierQuoteId,
                SupplierId = bestPricedOption.SupplierQuote.SupplierId,
                SupplierName = ResolveSupplierName(bestPricedOption.SupplierQuote.Supplier),
                Price = bestPricedOption.Price,
                TaxRate = requestDetail.Product?.TaxRate ?? 10m,
                Product = requestDetail.Product,
                SupplierQuoteDetail = bestPricedOption
            });
        }

        if (errors.Count > 0)
            return Result<ResolvedPurchaseOrderDraft>.Failure(PurchaseOrderError.InvalidProducts, errors, ErrorType.Validation);

        return Result<ResolvedPurchaseOrderDraft>.Success(new ResolvedPurchaseOrderDraft
        {
            PurchaseRequestId = purchaseRequestId,
            Details = resolvedLines
        });
    }

    private async Task<Result> ValidateCreateRequestAsync(CreatePurchaseOrderRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestRequired];

        if (request.Details == null || request.Details.Count == 0)
            errors[nameof(request.Details)] = [PurchaseOrderError.DetailsRequired];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        var purchaseRequestExists = await _context.PurchaseRequests.AnyAsync(pr => pr.Id == request.PurchaseRequestId);
        if (!purchaseRequestExists)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestNotFound];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    private Result<List<ResolvedPurchaseOrderLine>> ApplyOverrides(ResolvedPurchaseOrderDraft draft, List<PurchaseOrderDetailRequestDto> overrides)
    {
        var errors = new Dictionary<string, string[]>();

        if (overrides == null || overrides.Count == 0)
            return Result<List<ResolvedPurchaseOrderLine>>.Success(draft.Details);

        var detailOverrides = overrides
            .GroupBy(detail => detail.ProductId)
            .ToDictionary(group => group.Key, group => group.Last());

        var draftDetailMap = draft.Details.ToDictionary(detail => detail.ProductId, detail => detail);
        var resolvedLines = new List<ResolvedPurchaseOrderLine>();

        foreach (var draftDetail in draft.Details)
        {
            if (detailOverrides.TryGetValue(draftDetail.ProductId, out var overrideDetail))
            {
                if (overrideDetail.QuantityOrdered <= 0)
                {
                    errors[$"Details[{overrideDetail.ProductId}].QuantityOrdered"] = [PurchaseOrderError.InvalidQuantity];
                    continue;
                }

                draftDetail.QuantityOrdered = overrideDetail.QuantityOrdered;

                if (overrideDetail.SupplierQuoteDetailId.HasValue)
                {
                    var selectedQuoteDetail = _context.SupplierQuoteDetails
                        .AsNoTracking()
                        .Include(qd => qd.SupplierQuote)
                            .ThenInclude(sq => sq.Supplier)
                        .Include(qd => qd.Product)
                        .FirstOrDefault(qd => qd.Id == overrideDetail.SupplierQuoteDetailId.Value);

                    if (selectedQuoteDetail == null || selectedQuoteDetail.SupplierQuote == null || selectedQuoteDetail.SupplierQuote.PurchaseRequestId != draft.PurchaseRequestId || selectedQuoteDetail.ProductId != draftDetail.ProductId)
                    {
                        errors[$"Details[{overrideDetail.ProductId}].SupplierQuoteDetailId"] = [PurchaseOrderError.InvalidSupplierQuoteDetail];
                        continue;
                    }

                    draftDetail.SupplierQuoteDetailId = selectedQuoteDetail.Id;
                    draftDetail.SupplierQuoteId = selectedQuoteDetail.SupplierQuoteId;
                    draftDetail.SupplierId = selectedQuoteDetail.SupplierQuote.SupplierId;
                    draftDetail.SupplierName = ResolveSupplierName(selectedQuoteDetail.SupplierQuote.Supplier);
                    draftDetail.Price = selectedQuoteDetail.Price;
                    draftDetail.TaxRate = selectedQuoteDetail.Product?.TaxRate ?? 10m;
                }
            }

            resolvedLines.Add(new ResolvedPurchaseOrderLine
            {
                ProductId = draftDetail.ProductId,
                QuantityOrdered = draftDetail.QuantityOrdered,
                SupplierQuoteDetailId = draftDetail.SupplierQuoteDetailId,
                SupplierQuoteId = draftDetail.SupplierQuoteId,
                SupplierId = draftDetail.SupplierId,
                SupplierName = draftDetail.SupplierName,
                Price = draftDetail.Price,
                TaxRate = draftDetail.TaxRate,
                Product = draftDetail.Product,
                SupplierQuoteDetail = draftDetail.SupplierQuoteDetail
            });
        }

        foreach (var overrideDetail in detailOverrides.Values)
        {
            if (!draftDetailMap.ContainsKey(overrideDetail.ProductId))
                errors[$"Details[{overrideDetail.ProductId}].ProductId"] = [PurchaseOrderError.InvalidProducts];
        }

        if (errors.Count > 0)
            return Result<List<ResolvedPurchaseOrderLine>>.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result<List<ResolvedPurchaseOrderLine>>.Success(resolvedLines);
    }

    private PurchaseOrderDraftDto BuildDraftResponse(ResolvedPurchaseOrderDraft draft)
    {
        return new PurchaseOrderDraftDto
        {
            PurchaseRequestId = draft.PurchaseRequestId,
            Total = draft.Details.Sum(line => line.Price * line.QuantityOrdered),
            Details = draft.Details.Select(line => new PurchaseOrderDraftDetailDto
            {
                ProductId = line.ProductId,
                ProductName = line.Product?.Name,
                QuantityOrdered = line.QuantityOrdered,
                Price = line.Price,
                TaxRate = line.TaxRate,
                SupplierId = line.SupplierId ?? 0,
                SupplierName = line.SupplierName,
                SupplierQuoteDetailId = line.SupplierQuoteDetailId ?? 0,
                SupplierQuoteId = line.SupplierQuoteId ?? 0
            }).ToList()
        };
    }

    private static string? ResolveSupplierName(Supplier? supplier)
    {
        if (supplier == null)
            return null;

        return string.IsNullOrWhiteSpace(supplier.FantasyName) ? supplier.BusinessName : supplier.FantasyName;
    }

    private static string GeneratePurchaseOrderNumber(int id) => $"OC-{id:D6}";

    private static string GenerateChildPurchaseOrderNumber(string mainOrderNumber, int sequence)
        => $"OCP-{mainOrderNumber}-{sequence:D3}";

    private sealed class ResolvedPurchaseOrderLine
    {
        public int ProductId { get; set; }
        public decimal QuantityOrdered { get; set; }
        public int? SupplierQuoteDetailId { get; set; }
        public int? SupplierQuoteId { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal Price { get; set; }
        public decimal TaxRate { get; set; }
        public Product? Product { get; set; }
        public SupplierQuoteDetail? SupplierQuoteDetail { get; set; }
    }

    private sealed class ResolvedPurchaseOrderDraft
    {
        public int PurchaseRequestId { get; set; }
        public List<ResolvedPurchaseOrderLine> Details { get; set; } = [];
    }
}
