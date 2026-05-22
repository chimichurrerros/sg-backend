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
        var orders = await LoadOrdersQuery()
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders)
        });
    }

    public async Task<Result<ListPurchaseOrdersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = LoadOrdersQuery();
        var total = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders),
            Pagination = new Pagination(pagination.Page, pagination.PageSize, total)
        });
    }

    public async Task<Result<PurchaseOrderWrapperDto>> GetByIdAsync(int id)
    {
        var order = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(order));
    }

    public async Task<Result<PurchaseOrderDraftWrapperDto>> GetDraftByPurchaseRequestIdAsync(int purchaseRequestId)
    {
        var draftResult = await BuildDraftDataAsync(purchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderDraftWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        return Result<PurchaseOrderDraftWrapperDto>.Success(new PurchaseOrderDraftWrapperDto
        {
            PurchaseOrder = BuildDraftResponse(draftResult.Value!)
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

        var resolved = ApplyOverrides(draftResult.Value!, request.Details, request.SupplierId);
        if (!resolved.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(resolved.ErrorMessage!, resolved.Errors!, resolved.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new PurchaseOrder
            {
                SupplierId = resolved.Value!.PrimarySupplierId,
                PurchaseRequestId = request.PurchaseRequestId,
                SupplierQuoteId = resolved.Value.PrimarySupplierQuoteId,
                Number = string.Empty,
                Date = DateTime.UtcNow,
                Total = resolved.Value.Details.Sum(d => d.Price * d.QuantityOrdered),
                State = PurchaseOrder.PurchaseOrderStateEnum.Pending
            };

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();

            order.Number = GeneratePurchaseOrderNumber(order.Id);
            _context.PurchaseOrders.Update(order);
            await _context.SaveChangesAsync();

            foreach (var detail in resolved.Value.Details)
            {
                _context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                {
                    PurchaseOrderId = order.Id,
                    ProductId = detail.ProductId,
                    SupplierQuoteDetailId = detail.SupplierQuoteDetailId,
                    QuantityOrdered = detail.QuantityOrdered,
                    QuantityReceived = 0,
                    Price = detail.Price,
                    TaxRate = 0m
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var created = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == order.Id);
            return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(created));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<PurchaseOrderWrapperDto>> UpdateAsync(int id, UpdatePurchaseOrderRequestDto request)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.PurchaseOrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        var validation = await ValidateUpdateRequestAsync(request, order);
        if (!validation.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        var draftResult = await BuildDraftDataAsync(request.PurchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        var resolved = ApplyOverrides(draftResult.Value!, request.Details, request.SupplierId);
        if (!resolved.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(resolved.ErrorMessage!, resolved.Errors!, resolved.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            order.PurchaseRequestId = request.PurchaseRequestId;
            order.SupplierId = resolved.Value!.PrimarySupplierId;
            order.SupplierQuoteId = resolved.Value.PrimarySupplierQuoteId;
            order.Total = resolved.Value.Details.Sum(d => d.Price * d.QuantityOrdered);

            if (request.State.HasValue && Enum.IsDefined(typeof(PurchaseOrder.PurchaseOrderStateEnum), request.State.Value))
            {
                order.State = (PurchaseOrder.PurchaseOrderStateEnum)request.State.Value;
            }

            _context.PurchaseOrderDetails.RemoveRange(order.PurchaseOrderDetails);
            order.PurchaseOrderDetails = resolved.Value.Details.Select(detail => new PurchaseOrderDetail
            {
                ProductId = detail.ProductId,
                SupplierQuoteDetailId = detail.SupplierQuoteDetailId,
                QuantityOrdered = detail.QuantityOrdered,
                QuantityReceived = 0,
                Price = detail.Price,
                TaxRate = 0m
            }).ToList();

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updated = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == id);
            return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(updated));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> ConfirmPurchaseOrderAsync(int purchaseOrderId)
    {
        var order = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
        if (order == null)
            return Result<bool>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        order.State = PurchaseOrder.PurchaseOrderStateEnum.Confirmed;
        _context.PurchaseOrders.Update(order);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<ListSuppliersWrapperDto>> GetSuppliersByPurchaseOrderIdAsync(int purchaseOrderId)
    {
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.SupplierQuoteDetail)
                    .ThenInclude(sd => sd.SupplierQuote)
                        .ThenInclude(sq => sq.Supplier)
            .FirstOrDefaultAsync(o => o.Id == purchaseOrderId);

        if (order == null)
            return Result<ListSuppliersWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        // Retrieve the primary supplier
        var suppliers = new List<Supplier>();
        if (order.Supplier != null)
        {
            suppliers.Add(order.Supplier);
        }

        // Retrieve any other suppliers involved in the details
        var detailSuppliers = order.PurchaseOrderDetails
            .Where(d => d.SupplierQuoteDetail != null && d.SupplierQuoteDetail.SupplierQuote != null && d.SupplierQuoteDetail.SupplierQuote.Supplier != null)
            .Select(d => d.SupplierQuoteDetail!.SupplierQuote.Supplier)
            .ToList();
            
        suppliers.AddRange(detailSuppliers);

        // Get distinct suppliers by Id
        var distinctSuppliers = suppliers
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();

        var dtos = _mapper.Map<List<SupplierResponseDto>>(distinctSuppliers);

        return Result<ListSuppliersWrapperDto>.Success(new ListSuppliersWrapperDto
        {
            Suppliers = dtos,
            Pagination = new Pagination(1, dtos.Count, dtos.Count)
        });
    }

    private IQueryable<PurchaseOrder> LoadOrdersQuery()
    {
        return _context.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.PurchaseRequest)
            .Include(o => o.SupplierQuote)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.SupplierQuoteDetail)
                    .ThenInclude(sd => sd.SupplierQuote)
                        .ThenInclude(sq => sq.Supplier);
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

        // LOGICA MEJORADA: Para cada producto, buscar el mejor precio automaticamente
        foreach (var requestDetail in purchaseRequest.PurchaseRequestDetails)
        {
            // Obtener todos los precios disponibles para este producto
            var candidates = quoteDetails
                .Where(qd => qd.ProductId == requestDetail.ProductId)
                .OrderBy(qd => qd.Price)  // Ordenar por menor precio
                .ThenBy(qd => qd.SupplierQuoteId)  // Desempate por ID de cotización
                .ToList();

            if (candidates.Count == 0)
            {
                errors[$"PurchaseRequestDetails[{requestDetail.ProductId}]"] = [PurchaseOrderError.InvalidProducts];
                continue;
            }

            // Seleccionar automáticamente el de menor precio
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
                Product = requestDetail.Product,
                SupplierQuoteDetail = bestPricedOption
            });
        }

        if (errors.Count > 0)
            return Result<ResolvedPurchaseOrderDraft>.Failure(PurchaseOrderError.InvalidProducts, errors, ErrorType.Validation);

        // Determinar el proveedor "primario" basado en el precio total más bajo
        // Esto es solo para referencia en la orden, ya que cada producto tiene su mejor precio
        var primarySupplierGroup = resolvedLines
            .GroupBy(line => line.SupplierId ?? 0)
            .Select(group => new
            {
                SupplierId = group.Key,
                Total = group.Sum(line => line.Price * line.QuantityOrdered)
            })
            .OrderBy(group => group.Total)
            .FirstOrDefault();

        var primarySupplierId = primarySupplierGroup?.SupplierId ?? 0;
        var primarySupplier = primarySupplierId == 0
            ? null
            : resolvedLines.First(line => line.SupplierId == primarySupplierId).SupplierQuoteDetail!.SupplierQuote!.Supplier;

        // Si todos los productos vienen del mismo proveedor, usar esa cotización como referencia
        var allSameSupplier = resolvedLines.Select(line => line.SupplierId).Distinct().Count() == 1;
        var supplierQuoteId = allSameSupplier ? resolvedLines.First().SupplierQuoteId : null;

        return Result<ResolvedPurchaseOrderDraft>.Success(new ResolvedPurchaseOrderDraft
        {
            PurchaseRequestId = purchaseRequestId,
            PurchaseRequestState = purchaseRequest.PurchaseRequestState,
            PrimarySupplierId = primarySupplierId,
            PrimarySupplier = primarySupplier,
            PrimarySupplierQuoteId = supplierQuoteId,
            Details = resolvedLines
        });
    }

    private async Task<Result> ValidateCreateRequestAsync(CreatePurchaseOrderRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestRequired];

        if (request.SupplierId.HasValue && request.SupplierId.Value <= 0)
            errors[nameof(request.SupplierId)] = [PurchaseOrderError.SupplierRequired];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        var purchaseRequestExists = await _context.PurchaseRequests.AnyAsync(pr => pr.Id == request.PurchaseRequestId);
        if (!purchaseRequestExists)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestNotFound];

        if (request.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value);
            if (!supplierExists)
                errors[nameof(request.SupplierId)] = [PurchaseOrderError.InvalidSupplier];
        }

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdatePurchaseOrderRequestDto request, PurchaseOrder order)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestRequired];

        if (request.Details == null || request.Details.Count == 0)
            errors[nameof(request.Details)] = [PurchaseOrderError.DetailsRequired];

        if (request.SupplierId.HasValue && request.SupplierId.Value <= 0)
            errors[nameof(request.SupplierId)] = [PurchaseOrderError.SupplierRequired];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        if (request.PurchaseRequestId != order.PurchaseRequestId)
        {
            var purchaseRequestExists = await _context.PurchaseRequests.AnyAsync(pr => pr.Id == request.PurchaseRequestId);
            if (!purchaseRequestExists)
                errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestNotFound];
        }

        if (request.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value);
            if (!supplierExists)
                errors[nameof(request.SupplierId)] = [PurchaseOrderError.InvalidSupplier];
        }

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    private Result<ResolvedPurchaseOrderDraft> ApplyOverrides(ResolvedPurchaseOrderDraft draft, List<PurchaseOrderDetailRequestDto> overrides, int? preferredSupplierId)
    {
        var errors = new Dictionary<string, string[]>();

        // Si no se proporcionan overrides (detalles específicos), usar automáticamente los mejores precios
        if (overrides == null || overrides.Count == 0)
        {
            // Ya tenemos los mejores precios del draft, solo aplicar proveedor preferido si lo hay
            if (preferredSupplierId.HasValue && preferredSupplierId.Value > 0)
            {
                var primarySupplierExists = draft.Details.Any(d => d.SupplierId == preferredSupplierId.Value);
                if (!primarySupplierExists)
                {
                    errors[nameof(preferredSupplierId)] = [PurchaseOrderError.InvalidSupplier];
                    return Result<ResolvedPurchaseOrderDraft>.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);
                }
            }

            // Usar el draft tal cual con los mejores precios automáticamente seleccionados
            var finalPrimarySupplierId = preferredSupplierId.HasValue && preferredSupplierId.Value > 0
                ? preferredSupplierId.Value
                : draft.PrimarySupplierId;

            var allSameSupplier = draft.Details.Select(line => line.SupplierId).Distinct().Count() == 1;
            var primarySupplierQuoteId = allSameSupplier && draft.Details.Count > 0
                ? draft.Details.First().SupplierQuoteId
                : null;

            return Result<ResolvedPurchaseOrderDraft>.Success(new ResolvedPurchaseOrderDraft
            {
                PurchaseRequestId = draft.PurchaseRequestId,
                PurchaseRequestState = draft.PurchaseRequestState,
                PrimarySupplierId = finalPrimarySupplierId,
                PrimarySupplier = draft.PrimarySupplier,
                PrimarySupplierQuoteId = primarySupplierQuoteId,
                Details = draft.Details
            });
        }

        // Si se proporcionan overrides, procesarlos normalmente
        var detailOverrides = overrides
            .GroupBy(detail => detail.ProductId)
            .ToDictionary(group => group.Key, group => group.Last());

        var requestDetailMap = draft.Details.ToDictionary(detail => detail.ProductId, detail => detail);

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
                Product = draftDetail.Product,
                SupplierQuoteDetail = draftDetail.SupplierQuoteDetail
            });
        }

        foreach (var overrideDetail in detailOverrides.Values)
        {
            if (!requestDetailMap.ContainsKey(overrideDetail.ProductId))
                errors[$"Details[{overrideDetail.ProductId}].ProductId"] = [PurchaseOrderError.InvalidProducts];
        }

        if (errors.Count > 0)
            return Result<ResolvedPurchaseOrderDraft>.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        var overridePrimarySupplierId = preferredSupplierId.HasValue && preferredSupplierId.Value > 0
            ? preferredSupplierId.Value
            : resolvedLines
            .GroupBy(line => line.SupplierId ?? 0)
            .Select(group => new { SupplierId = group.Key, Total = group.Sum(line => line.Price * line.QuantityOrdered) })
                .OrderBy(group => group.Total)
                .First().SupplierId;

        var allSameQuote = resolvedLines.Select(line => line.SupplierQuoteId).Distinct().Count() == 1;
        var overridePrimarySupplierQuoteId = allSameQuote ? resolvedLines.First().SupplierQuoteId : null;

        return Result<ResolvedPurchaseOrderDraft>.Success(new ResolvedPurchaseOrderDraft
        {
            PurchaseRequestId = draft.PurchaseRequestId,
            PurchaseRequestState = draft.PurchaseRequestState,
            PrimarySupplierId = overridePrimarySupplierId,
            PrimarySupplier = draft.PrimarySupplier,
            PrimarySupplierQuoteId = overridePrimarySupplierQuoteId,
            Details = resolvedLines
        });
    }

    private PurchaseOrderResponseDto BuildDraftResponse(ResolvedPurchaseOrderDraft draft)
    {
        var order = new PurchaseOrder
        {
            Id = 0,
            PurchaseRequestId = draft.PurchaseRequestId,
            SupplierId = draft.PrimarySupplierId,
            Supplier = draft.PrimarySupplier ?? new Supplier { Id = draft.PrimarySupplierId, BusinessName = string.Empty },
            SupplierQuoteId = draft.PrimarySupplierQuoteId,
            Number = string.Empty,
            Date = DateTime.UtcNow,
            Total = draft.Details.Sum(line => line.Price * line.QuantityOrdered),
            State = PurchaseOrder.PurchaseOrderStateEnum.Pending,
            PurchaseOrderDetails = draft.Details.Select(line => new PurchaseOrderDetail
            {
                ProductId = line.ProductId,
                QuantityOrdered = line.QuantityOrdered,
                QuantityReceived = 0,
                Price = line.Price,
                TaxRate = 0m,
                SupplierQuoteDetailId = line.SupplierQuoteDetailId,
                Product = line.Product!,
                SupplierQuoteDetail = line.SupplierQuoteDetail
            }).ToList()
        };

        return _mapper.Map<PurchaseOrderResponseDto>(order);
    }

    private static string? ResolveSupplierName(Supplier? supplier)
    {
        if (supplier == null)
            return null;

        return string.IsNullOrWhiteSpace(supplier.FantasyName) ? supplier.BusinessName : supplier.FantasyName;
    }

    private static string GeneratePurchaseOrderNumber(int id) => $"OC-{id:D6}";

    private sealed class ResolvedPurchaseOrderLine
    {
        public int ProductId { get; set; }
        public decimal QuantityOrdered { get; set; }
        public int? SupplierQuoteDetailId { get; set; }
        public int? SupplierQuoteId { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal Price { get; set; }
        public Product? Product { get; set; }
        public SupplierQuoteDetail? SupplierQuoteDetail { get; set; }
    }

    private sealed class ResolvedPurchaseOrderDraft
    {
        public int PurchaseRequestId { get; set; }
        public PurchaseRequestStateEnum PurchaseRequestState { get; set; }
        public int PrimarySupplierId { get; set; }
        public Supplier? PrimarySupplier { get; set; }
        public int? PrimarySupplierQuoteId { get; set; }
        public List<ResolvedPurchaseOrderLine> Details { get; set; } = [];
    }
}
