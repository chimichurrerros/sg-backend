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

    // Obtiene absolutamente todas las cotizaciones registradas
    public async Task<Result<ListSupplierQuotesWrapperDto>> GetAllAsync()
    {
        // Busca en la base de datos trayendo toda la información relacionada de cada cotización
        var quotes = await _context.SupplierQuotes
            .AsNoTracking() // No hace seguimiento de cambios para que cargue más rápido
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(q => q.Id) // Las ordena de la más nueva a la más vieja
            .ToListAsync();

        // Convierte la lista de entidades al formato de respuesta (DTO)
        var dtos = _mapper.Map<List<SupplierQuoteResponseDto>>(quotes);

        // Retorna el resultado exitoso con la lista
        return Result<ListSupplierQuotesWrapperDto>.Success(new ListSupplierQuotesWrapperDto
        {
            SupplierQuotes = dtos
        });
    }

    // Obtiene una lista de cotizaciones pero dividida en páginas (paginada)
    public async Task<Result<ListSupplierQuotesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        // Prepara la consulta base con todas las relaciones necesarias
        var query = _context.SupplierQuotes
            .AsNoTracking()
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product);

        // Cuenta el total de cotizaciones en la base de datos
        var total = await query.CountAsync();

        // Trae solo las cotizaciones de la página solicitada
        var quotes = await query
            .OrderByDescending(q => q.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize) // Se salta las de páginas anteriores
            .Take(pagination.PageSize) // Toma solo la cantidad por página
            .ToListAsync();

        // Convierte las cotizaciones a DTO
        var dtos = _mapper.Map<List<SupplierQuoteResponseDto>>(quotes);
        
        // Arma la información de la paginación (página actual, tamaño, total)
        var paginationData = new Pagination(pagination.Page, pagination.PageSize, total);

        // Retorna la lista de esa página y los datos de paginación
        return Result<ListSupplierQuotesWrapperDto>.Success(new ListSupplierQuotesWrapperDto
        {
            SupplierQuotes = dtos,
            Pagination = paginationData
        });
    }

    // Obtiene una cotización específica usando su ID
    public async Task<Result<SupplierQuoteWrapperDto>> GetByIdAsync(int id)
    {
        // Busca en la base de datos trayendo proveedor, órdenes, solicitudes y productos
        var quote = await _context.SupplierQuotes
            .AsNoTracking() // Evita rastrear cambios para que la consulta sea más rápida
            .Include(q => q.Supplier)
            .Include(q => q.PurchaseOrders)
            .Include(q => q.PurchaseRequest)
            .Include(q => q.SupplierQuoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.Id == id); // Filtra por el ID recibido

        // Si no la encuentra, devuelve un error de no encontrado (404)
        if (quote == null)
            return Result<SupplierQuoteWrapperDto>.Failure(SupplierQuoteError.SupplierQuoteNotFound, ErrorType.NotFound);

        // Si la encuentra, la convierte a DTO (formato de respuesta) y la devuelve con éxito
        return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(quote));
    }

    // Crea una nueva cotización de proveedor
    public async Task<Result<SupplierQuoteWrapperDto>> CreateAsync(CreateSupplierQuoteRequestDto request)
    {
        // Valida que los datos recibidos sean correctos
        var validation = await ValidateCreateRequestAsync(request);
        if (!validation.IsSuccess)
            return Result<SupplierQuoteWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        // Inicia una transacción para asegurar que todo se guarde bien o nada se guarde (evita datos a medias)
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Convierte los datos recibidos a la entidad de la base de datos
            var quote = _mapper.Map<SupplierQuote>(request);
            quote.Date = DateTime.UtcNow; // Asigna la fecha y hora actual en UTC
            quote.Total = CalculateTotal(request.Details); // Calcula el costo total de la cotización

            // Agrega la cotización al contexto y guarda los cambios en la base de datos
            _context.SupplierQuotes.Add(quote);
            await _context.SaveChangesAsync();

            // Confirma y guarda permanentemente la transacción
            await transaction.CommitAsync();

            // Busca la cotización recién creada con todas sus relaciones para devolverla completa
            var created = await _context.SupplierQuotes
                .AsNoTracking()
                .Include(q => q.Supplier)
                .Include(q => q.PurchaseOrders)
                .Include(q => q.PurchaseRequest)
                .Include(q => q.SupplierQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == quote.Id);

            return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(created));
        }
        catch
        {
            // Si algo falla, deshace todos los cambios realizados en esta transacción
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Actualiza una cotización existente
    public async Task<Result<SupplierQuoteWrapperDto>> UpdateAsync(int id, UpdateSupplierQuoteRequestDto request)
    {
        // Busca la cotización a modificar junto con sus detalles actuales
        var quote = await _context.SupplierQuotes
            .Include(q => q.SupplierQuoteDetails)
            .FirstOrDefaultAsync(q => q.Id == id);

        // Si no existe, devuelve error 404
        if (quote == null)
            return Result<SupplierQuoteWrapperDto>.Failure(SupplierQuoteError.SupplierQuoteNotFound, ErrorType.NotFound);

        // Valida que los cambios solicitados sean válidos
        var validation = await ValidateUpdateRequestAsync(request, id, quote.SupplierId, quote.PurchaseRequestId);
        if (!validation.IsSuccess)
            return Result<SupplierQuoteWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        // Inicia una transacción para asegurar la consistencia al modificar detalles
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Si viene un nuevo proveedor, lo actualiza
            if (request.SupplierId.HasValue)
                quote.SupplierId = request.SupplierId.Value;

            // Si viene una nueva solicitud de compra, la actualiza
            if (request.PurchaseRequestId.HasValue)
                quote.PurchaseRequestId = request.PurchaseRequestId.Value;

            // Si se envían nuevos detalles de productos
            if (request.Details != null)
            {
                // Borra los detalles viejos de la base de datos
                _context.SupplierQuoteDetails.RemoveRange(quote.SupplierQuoteDetails);
                
                // Crea y asigna los nuevos detalles con impuesto del 10% por defecto
                quote.SupplierQuoteDetails = request.Details
                    .Select(d => new SupplierQuoteDetail
                    {
                        ProductId = d.ProductId,
                        QuantityAvailable = d.QuantityAvailable,
                        Price = d.Price
                    })
                    .ToList();
                
                // Vuelve a calcular el total de la cotización
                quote.Total = CalculateTotal(request.Details);
            }

            // Si viene un nuevo estado de cotización válido, lo actualiza
            if (request.State.HasValue && Enum.IsDefined(typeof(SupplierQuoteStateEnum), request.State.Value))
            {
                quote.State = (SupplierQuoteStateEnum)request.State.Value;
            }

            // Guarda los cambios y confirma la transacción
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Trae la cotización actualizada con todas sus relaciones cargadas
            var updated = await _context.SupplierQuotes
                .AsNoTracking()
                .Include(q => q.Supplier)
                .Include(q => q.PurchaseOrders)
                .Include(q => q.PurchaseRequest)
                .Include(q => q.SupplierQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            return Result<SupplierQuoteWrapperDto>.Success(_mapper.Map<SupplierQuoteWrapperDto>(updated));
        }
        catch
        {
            // Si hay un error, deshace los cambios para no dejar datos corruptos
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Calcula el total sumando la multiplicación de cantidad disponible por precio de cada detalle
    private static decimal CalculateTotal(List<SupplierQuoteDetailRequestDto> details)
    {
        return details.Sum(d => d.QuantityAvailable * d.Price);
    }

    // Valida los datos al crear una cotización
    private async Task<Result> ValidateCreateRequestAsync(CreateSupplierQuoteRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        // Verifica que el proveedor y la solicitud sean válidos
        if (request.SupplierId <= 0)
            errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierIdRequired];

        if (request.PurchaseRequestId <= 0)
            errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestIdRequired];

        // Valida la lista de detalles (productos, cantidades, precios)
        ValidateDetails(request.Details, errors, nameof(request.Details));

        // Si hay errores de formato, los devuelve inmediatamente
        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        // Verifica que el proveedor exista realmente en la base de datos
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
        if (!supplierExists)
            errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierNotFound];

        // Verifica que la solicitud de compra exista en la base de datos
        var prExists = await _context.PurchaseRequests.AnyAsync(p => p.Id == request.PurchaseRequestId);
        if (!prExists)
            errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestNotFound];

        // Valida que todos los productos de la cotización estén incluidos en la solicitud de compra original
        var productsValidation = await ValidateProductsBelongToPurchaseRequestAsync(request.PurchaseRequestId, request.Details.Select(d => d.ProductId).ToList());
        if (!productsValidation.IsSuccess)
            errors[nameof(request.Details)] = [SupplierQuoteError.InvalidProducts];

        // Si hubo algún error de existencia o pertenencia, los devuelve
        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    // Valida los datos al actualizar una cotización existente
    private async Task<Result> ValidateUpdateRequestAsync(UpdateSupplierQuoteRequestDto request, int quoteId, int currentSupplierId, int currentPurchaseRequestId)
    {
        var errors = new Dictionary<string, string[]>();

        // Si se envió un proveedor, valida que sea válido y exista
        if (request.SupplierId.HasValue)
        {
            if (request.SupplierId <= 0)
                errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierIdRequired];
            else if (request.SupplierId != currentSupplierId)
            {
                var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
                if (!supplierExists)
                    errors[nameof(request.SupplierId)] = [SupplierQuoteError.SupplierNotFound];
            }
        }

        // Si se envió una solicitud de compra, valida que sea válida y exista
        if (request.PurchaseRequestId.HasValue)
        {
            if (request.PurchaseRequestId <= 0)
                errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestIdRequired];
            else if (request.PurchaseRequestId != currentPurchaseRequestId)
            {
                var prExists = await _context.PurchaseRequests.AnyAsync(p => p.Id == request.PurchaseRequestId);
                if (!prExists)
                    errors[nameof(request.PurchaseRequestId)] = [SupplierQuoteError.PurchaseRequestNotFound];
            }
        }

        // Si se enviaron detalles nuevos
        if (request.Details != null)
        {
            // No se pueden cambiar los productos si esta cotización ya tiene órdenes de compra asociadas
            var hasAssociatedPurchaseOrders = await _context.SupplierQuoteDetails
                .Where(d => d.SupplierQuoteId == quoteId)
                .SelectMany(d => d.PurchaseOrderDetails)
                .AnyAsync();

            if (hasAssociatedPurchaseOrders)
            {
                errors[nameof(request.Details)] = ["No se pueden actualizar los detalles de una cotización que ya tiene órdenes de compra asociadas."];
            }
            else
            {
                // Valida cantidades y precios de los detalles
                ValidateDetails(request.Details, errors, nameof(request.Details));

                // Si no hay errores básicos, valida que pertenezcan a la solicitud de compra
                if (!errors.ContainsKey(nameof(request.Details)))
                {
                    var purchaseRequestIdToValidate = request.PurchaseRequestId ?? currentPurchaseRequestId;
                    var productsValidation = await ValidateProductsBelongToPurchaseRequestAsync(purchaseRequestIdToValidate, request.Details.Select(d => d.ProductId).ToList());
                    if (!productsValidation.IsSuccess)
                        errors[nameof(request.Details)] = [SupplierQuoteError.InvalidProducts];
                }
            }
        }

        // Si se encontraron errores, los devuelve
        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors.Values.SelectMany(v => v)), errors, ErrorType.Validation);

        return Result.Success();
    }

    // Valida que la lista de detalles no esté vacía, y que las cantidades/precios no sean menores a cero
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

    // Verifica que los productos cotizados pertenezcan realmente a la solicitud de compra original
    private async Task<Result> ValidateProductsBelongToPurchaseRequestAsync(int purchaseRequestId, List<int> productIds)
    {
        var distinct = productIds.Distinct().ToList();
        if (distinct.Count == 0)
            return Result.Failure(SupplierQuoteError.InvalidProducts, ErrorType.Validation);

        // Cuenta cuántos de esos productos coinciden con los detalles de la solicitud de compra
        var count = await _context.PurchaseRequestDetails
            .CountAsync(d => d.PurchaseRequestId == purchaseRequestId && distinct.Contains(d.ProductId));

        // Si la cantidad coincide, todos son correctos; si no, hay productos inválidos
        if (count != distinct.Count)
            return Result.Failure(SupplierQuoteError.InvalidProducts, ErrorType.Validation);

        return Result.Success();
    }
}
