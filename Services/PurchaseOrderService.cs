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

    // Obtiene absolutamente todas las órdenes de compra con sus datos relacionados
    public async Task<Result<ListPurchaseOrdersWrapperDto>> GetAllAsync()
    {
        // Consulta las órdenes de compra y las ordena de la más nueva a la más vieja
        var orders = await LoadOrdersQuery()
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        // Mapea las entidades al formato de respuesta (DTO) y las devuelve
        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders)
        });
    }

    // Obtiene una lista de órdenes de compra pero dividida en páginas (paginada)
    public async Task<Result<ListPurchaseOrdersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = LoadOrdersQuery();
        // Cuenta cuántas órdenes de compra hay en total en la base de datos
        var total = await query.CountAsync();

        // Trae únicamente las órdenes correspondientes a la página solicitada
        var orders = await query
            .OrderByDescending(o => o.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize) // Se salta las páginas anteriores
            .Take(pagination.PageSize) // Toma solo la cantidad definida para la página
            .ToListAsync();

        // Devuelve la lista de órdenes mapeada junto con la información de paginación
        return Result<ListPurchaseOrdersWrapperDto>.Success(new ListPurchaseOrdersWrapperDto
        {
            PurchaseOrders = _mapper.Map<List<PurchaseOrderResponseDto>>(orders),
            Pagination = new Pagination(pagination.Page, pagination.PageSize, total)
        });
    }

    // Busca una orden de compra específica mediante su ID
    public async Task<Result<PurchaseOrderWrapperDto>> GetByIdAsync(int id)
    {
        // Busca en la base de datos cargando todas las relaciones
        var order = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == id);

        // Si no existe, devuelve un error 404
        if (order == null)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        // Si la encuentra, la mapea a DTO y la retorna
        return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(order));
    }

    // Obtiene un borrador (draft) de orden de compra basado en una solicitud de compra
    public async Task<Result<PurchaseOrderDraftWrapperDto>> GetDraftByPurchaseRequestIdAsync(int purchaseRequestId)
    {
        // Arma los datos del borrador eligiendo los mejores precios disponibles automáticamente
        var draftResult = await BuildDraftDataAsync(purchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderDraftWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        // Envuelve la orden borrador mapeada en el DTO de respuesta y la retorna
        return Result<PurchaseOrderDraftWrapperDto>.Success(new PurchaseOrderDraftWrapperDto
        {
            PurchaseOrder = BuildDraftResponse(draftResult.Value!)
        });
    }

    // Crea una nueva orden de compra
    public async Task<Result<PurchaseOrderWrapperDto>> CreateAsync(CreatePurchaseOrderRequestDto request)
    {
        // Valida que la solicitud de creación tenga datos correctos
        var validation = await ValidateCreateRequestAsync(request);
        if (!validation.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        // Construye los datos base (borrador) con los mejores precios automáticamente
        var draftResult = await BuildDraftDataAsync(request.PurchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        // Aplica cambios manuales sobre las cantidades o cotizaciones si el usuario lo especificó
        var resolved = ApplyOverrides(draftResult.Value!, request.Details, request.SupplierId);
        if (!resolved.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(resolved.ErrorMessage!, resolved.Errors!, resolved.ErrorType);

        // Inicia una transacción de base de datos para asegurar consistencia
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Crea la cabecera de la orden de compra
            var order = new PurchaseOrder
            {
                SupplierId = resolved.Value!.PrimarySupplierId,
                PurchaseRequestId = request.PurchaseRequestId,
                SupplierQuoteId = resolved.Value.PrimarySupplierQuoteId,
                Number = string.Empty, // Se genera temporalmente vacío
                Date = DateTime.UtcNow,
                Total = resolved.Value.Details.Sum(d => d.Price * d.QuantityOrdered), // Suma total de los productos
                State = PurchaseOrderStateEnum.Pending
            };

            // Guarda para obtener el ID asignado por la base de datos
            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();

            // Genera el número correlativo con formato (ej. OC-000123) usando el ID generado
            order.Number = GeneratePurchaseOrderNumber(order.Id);
            _context.PurchaseOrders.Update(order);
            await _context.SaveChangesAsync();

            // Inserta cada uno de los detalles de la orden de compra
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
                    TaxRate = detail.TaxRate
                });
            }

            // Guarda los detalles y confirma la transacción de forma definitiva
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Recupera la orden de compra recién creada con todos sus datos y relaciones mapeadas
            var created = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == order.Id);
            return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(created));
        }
        catch
        {
            // Si algo sale mal, revierte todos los cambios hechos en esta transacción
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Actualiza una orden de compra existente
    public async Task<Result<PurchaseOrderWrapperDto>> UpdateAsync(int id, UpdatePurchaseOrderRequestDto request)
    {
        // Busca la orden de compra y sus detalles actuales
        var order = await _context.PurchaseOrders
            .Include(o => o.PurchaseOrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);

        // Si no la encuentra, devuelve error 404
        if (order == null)
            return Result<PurchaseOrderWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        // Valida la solicitud de actualización
        var validation = await ValidateUpdateRequestAsync(request, order);
        if (!validation.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        // Genera los datos base de la solicitud de compra asociada
        var draftResult = await BuildDraftDataAsync(request.PurchaseRequestId);
        if (!draftResult.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(draftResult.ErrorMessage!, draftResult.Errors!, draftResult.ErrorType);

        // Aplica modificaciones o elecciones manuales de cotizaciones y cantidades
        var resolved = ApplyOverrides(draftResult.Value!, request.Details, request.SupplierId);
        if (!resolved.IsSuccess)
            return Result<PurchaseOrderWrapperDto>.Failure(resolved.ErrorMessage!, resolved.Errors!, resolved.ErrorType);

        // Inicia una transacción para actualizar cabecera y detalles de manera segura
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Actualiza datos de cabecera
            order.PurchaseRequestId = request.PurchaseRequestId;
            order.SupplierId = resolved.Value!.PrimarySupplierId;
            order.SupplierQuoteId = resolved.Value.PrimarySupplierQuoteId;
            order.Total = resolved.Value.Details.Sum(d => d.Price * d.QuantityOrdered);

            // Actualiza el estado si se especificó uno válido
            if (request.State.HasValue && Enum.IsDefined(typeof(PurchaseOrderStateEnum), request.State.Value))
            {
                order.State = (PurchaseOrderStateEnum)request.State.Value;
            }

            // Remueve los detalles viejos
            _context.PurchaseOrderDetails.RemoveRange(order.PurchaseOrderDetails);

            // Asigna los nuevos detalles recalculados
            order.PurchaseOrderDetails = resolved.Value.Details.Select(detail => new PurchaseOrderDetail
            {
                ProductId = detail.ProductId,
                SupplierQuoteDetailId = detail.SupplierQuoteDetailId,
                QuantityOrdered = detail.QuantityOrdered,
                QuantityReceived = 0,
                Price = detail.Price,
                TaxRate = detail.TaxRate
            }).ToList();

            // Guarda los cambios y confirma la transacción
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Recupera la orden actualizada completa
            var updated = await LoadOrdersQuery().FirstOrDefaultAsync(o => o.Id == id);
            return Result<PurchaseOrderWrapperDto>.Success(_mapper.Map<PurchaseOrderWrapperDto>(updated));
        }
        catch
        {
            // Si hay error, deshace los cambios
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Confirma una orden de compra (cambia su estado a Confirmed)
    public async Task<Result<bool>> ConfirmPurchaseOrderAsync(int purchaseOrderId)
    {
        var order = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
        if (order == null)
            return Result<bool>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        order.State = PurchaseOrderStateEnum.Confirmed;
        _context.PurchaseOrders.Update(order);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // Obtiene todos los proveedores involucrados en una orden de compra específica
    public async Task<Result<ListSuppliersWrapperDto>> GetSuppliersByPurchaseOrderIdAsync(int purchaseOrderId)
    {
        // Busca la orden cargando los detalles y los proveedores vinculados a las cotizaciones de esos detalles
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.SupplierQuoteDetail)
                    .ThenInclude(sd => sd!.SupplierQuote)
                        .ThenInclude(sq => sq.Supplier)
            .FirstOrDefaultAsync(o => o.Id == purchaseOrderId);

        if (order == null)
            return Result<ListSuppliersWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        var suppliers = new List<Supplier>();

        // Agrega el proveedor principal de la orden de compra
        if (order.Supplier != null)
        {
            suppliers.Add(order.Supplier);
        }

        // Agrega los proveedores de cada cotización asociada a los detalles
        var detailSuppliers = order.PurchaseOrderDetails
            .Where(d => d.SupplierQuoteDetail != null && d.SupplierQuoteDetail.SupplierQuote != null && d.SupplierQuoteDetail.SupplierQuote.Supplier != null)
            .Select(d => d.SupplierQuoteDetail!.SupplierQuote.Supplier)
            .ToList();

        suppliers.AddRange(detailSuppliers);

        // Elimina duplicados basándose en el ID del proveedor
        var distinctSuppliers = suppliers
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();

        var dtos = _mapper.Map<List<SupplierResponseDto>>(distinctSuppliers);

        // Retorna la lista única de proveedores involucrados
        return Result<ListSuppliersWrapperDto>.Success(new ListSuppliersWrapperDto
        {
            Suppliers = dtos,
            Pagination = new Pagination(1, dtos.Count, dtos.Count)
        });
    }

    // Método auxiliar para preparar consultas de órdenes de compra con todas sus relaciones incluidas
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
                    .ThenInclude(sd => sd!.SupplierQuote)
                        .ThenInclude(sq => sq.Supplier);
    }

    // Construye la lógica del borrador, seleccionando automáticamente la cotización de mejor precio para cada producto
    private async Task<Result<ResolvedPurchaseOrderDraft>> BuildDraftDataAsync(int purchaseRequestId)
    {
        // Busca la solicitud de compra origen con sus productos y cantidades solicitadas
        var purchaseRequest = await _context.PurchaseRequests
            .AsNoTracking()
            .Include(pr => pr.PurchaseRequestDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

        if (purchaseRequest == null)
            return Result<ResolvedPurchaseOrderDraft>.Failure(PurchaseOrderError.PurchaseRequestNotFound, ErrorType.NotFound);

        // Busca todas las cotizaciones de proveedores que respondieron a esta solicitud de compra
        var quoteDetails = await _context.SupplierQuoteDetails
            .AsNoTracking()
            .Include(qd => qd.Product)
            .Include(qd => qd.SupplierQuote)
                .ThenInclude(sq => sq.Supplier)
            .Where(qd => qd.SupplierQuote.PurchaseRequestId == purchaseRequestId)
            .ToListAsync();

        var resolvedLines = new List<ResolvedPurchaseOrderLine>();
        var errors = new Dictionary<string, string[]>();

        // Para cada producto solicitado, buscamos la opción de menor precio
        foreach (var requestDetail in purchaseRequest.PurchaseRequestDetails)
        {
            // Filtra y ordena los candidatos que cotizaron este producto de menor a mayor precio
            var candidates = quoteDetails
                .Where(qd => qd.ProductId == requestDetail.ProductId)
                .OrderBy(qd => qd.Price)  // Menor precio primero
                .ThenBy(qd => qd.SupplierQuoteId)  // Desempate por ID de cotización
                .ToList();

            // Si nadie cotizó este producto, genera un error de validación
            if (candidates.Count == 0)
            {
                errors[$"PurchaseRequestDetails[{requestDetail.ProductId}]"] = [PurchaseOrderError.InvalidProducts];
                continue;
            }

            // Selecciona de forma automática la opción más barata
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

        // Determina cuál es el proveedor "principal" (el que suma menor costo total por sus productos)
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

        // Si todos los productos resultan ser del mismo proveedor, guardamos su cotización de referencia
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

    // Valida los datos requeridos para la creación de una orden de compra
    private async Task<Result> ValidateCreateRequestAsync(CreatePurchaseOrderRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestRequired];

        if (request.SupplierId.HasValue && request.SupplierId.Value <= 0)
            errors[nameof(request.SupplierId)] = [PurchaseOrderError.SupplierRequired];

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        // Valida que la solicitud de compra realmente exista en la base de datos
        var purchaseRequestExists = await _context.PurchaseRequests.AnyAsync(pr => pr.Id == request.PurchaseRequestId);
        if (!purchaseRequestExists)
            errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestNotFound];

        // Si se especificó un proveedor preferido, valida que exista en el sistema
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

    // Valida los datos requeridos para actualizar una orden de compra existente
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

        // Valida la solicitud de compra si cambió en la edición
        if (request.PurchaseRequestId != order.PurchaseRequestId)
        {
            var purchaseRequestExists = await _context.PurchaseRequests.AnyAsync(pr => pr.Id == request.PurchaseRequestId);
            if (!purchaseRequestExists)
                errors[nameof(request.PurchaseRequestId)] = [PurchaseOrderError.PurchaseRequestNotFound];
        }

        // Valida la existencia del proveedor
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

    // Aplica las elecciones manuales del usuario sobre el borrador automático (modificaciones a cotizaciones o cantidades)
    private Result<ResolvedPurchaseOrderDraft> ApplyOverrides(ResolvedPurchaseOrderDraft draft, List<PurchaseOrderDetailRequestDto> overrides, int? preferredSupplierId)
    {
        var errors = new Dictionary<string, string[]>();

        // Si no se pasaron modificaciones manuales, se conserva la selección automática de mejores precios
        if (overrides == null || overrides.Count == 0)
        {
            // Valida que el proveedor preferido exista dentro de las opciones cotizadas
            if (preferredSupplierId.HasValue && preferredSupplierId.Value > 0)
            {
                var primarySupplierExists = draft.Details.Any(d => d.SupplierId == preferredSupplierId.Value);
                if (!primarySupplierExists)
                {
                    errors[nameof(preferredSupplierId)] = [PurchaseOrderError.InvalidSupplier];
                    return Result<ResolvedPurchaseOrderDraft>.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);
                }
            }

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

        // Si se especificaron modificaciones manuales por producto, las procesa e integra al borrador
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

                // Modifica la cantidad ordenada
                draftDetail.QuantityOrdered = overrideDetail.QuantityOrdered;

                // Si se forzó una cotización de proveedor específica, se carga y aplican sus precios y datos
                if (overrideDetail.SupplierQuoteDetailId.HasValue)
                {
                    var selectedQuoteDetail = _context.SupplierQuoteDetails
                        .AsNoTracking()
                        .Include(qd => qd.SupplierQuote)
                            .ThenInclude(sq => sq.Supplier)
                        .Include(qd => qd.Product)
                        .FirstOrDefault(qd => qd.Id == overrideDetail.SupplierQuoteDetailId.Value);

                    // Valida que pertenezca a la solicitud de compra correcta y corresponda al producto correcto
                    if (selectedQuoteDetail == null || selectedQuoteDetail.SupplierQuote == null || selectedQuoteDetail.SupplierQuote.PurchaseRequestId != draft.PurchaseRequestId || selectedQuoteDetail.ProductId != draftDetail.ProductId)
                    {
                        errors[$"Details[{overrideDetail.ProductId}].SupplierQuoteDetailId"] = [PurchaseOrderError.InvalidSupplierQuoteDetail];
                        continue;
                    }

                    // Sobrescribe los datos automáticos con los de la cotización seleccionada a mano
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

        // Valida que no se intenten modificar productos inexistentes en la solicitud de compra
        foreach (var overrideDetail in detailOverrides.Values)
        {
            if (!requestDetailMap.ContainsKey(overrideDetail.ProductId))
                errors[$"Details[{overrideDetail.ProductId}].ProductId"] = [PurchaseOrderError.InvalidProducts];
        }

        if (errors.Count > 0)
            return Result<ResolvedPurchaseOrderDraft>.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        // Recalcula el proveedor principal tras aplicar los cambios manuales
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

    // Traduce los datos del borrador resuelto a una entidad de orden de compra temporal para mapearla a la respuesta
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
            State = PurchaseOrderStateEnum.Pending,
            PurchaseOrderDetails = draft.Details.Select(line => new PurchaseOrderDetail
            {
                ProductId = line.ProductId,
                QuantityOrdered = line.QuantityOrdered,
                QuantityReceived = 0,
                Price = line.Price,
                TaxRate = line.TaxRate,
                SupplierQuoteDetailId = line.SupplierQuoteDetailId,
                Product = line.Product!,
                SupplierQuoteDetail = line.SupplierQuoteDetail
            }).ToList()
        };

        return _mapper.Map<PurchaseOrderResponseDto>(order);
    }

    // Método auxiliar para resolver el nombre a mostrar del proveedor (nombre fantasía o razón social)
    private static string? ResolveSupplierName(Supplier? supplier)
    {
        if (supplier == null)
            return null;

        return string.IsNullOrWhiteSpace(supplier.FantasyName) ? supplier.BusinessName : supplier.FantasyName;
    }

    // Genera el formato de número correlativo para la orden de compra (ej. OC-000005)
    private static string GeneratePurchaseOrderNumber(int id) => $"OC-{id:D6}";

    // Clase auxiliar interna para guardar los detalles individuales resueltos del borrador
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

    // Clase auxiliar interna para mantener la estructura completa de un borrador resuelto
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
