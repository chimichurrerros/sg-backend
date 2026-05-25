using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseRequest;
using BackEnd.DTOs.Responses.PurchaseRequest;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace BackEnd.Services;

public class PurchaseRequestService(
    AppDbContext context,
    IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PurchaseRequestWrapperDto>> CreateAsync(CreatePurchaseRequestDto request, int userId)
    {
        // 1. Validamos que la solicitud de compra tenga al menos un detalle (producto y cantidad) especificado.
        if (request.Details == null || request.Details.Count == 0)
            return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.DetailsRequired, ErrorType.Validation);

        // 2. Iniciamos una transacción de base de datos para asegurar consistencia (si falla algo, revertimos todo).
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 3. Creamos la cabecera de la solicitud de compra, inicialmente en estado "Pendiente".
            var purchaseRequest = new PurchaseRequest
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                PurchaseRequestState = PurchaseRequestStateEnum.Pending,
                Observation = request.Observation
            };

            // 4. Guardamos la cabecera en la base de datos para obtener su ID autogenerado.
            _context.PurchaseRequests.Add(purchaseRequest);
            await _context.SaveChangesAsync();

            // 5. Procesamos cada detalle enviado en la solicitud.
            foreach (var detail in request.Details)
            {
                // Buscamos el producto en la base de datos.
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == detail.ProductId);
                if (product == null)
                {
                    // Si el producto no existe, revertimos los cambios y retornamos error.
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.ProductNotFound, ErrorType.Validation);
                }

                // Verificamos que el producto no sea un servicio (sólo se permiten bienes tangibles).
                if (product.IsService == true)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure($"El producto '{product.Name}' es un servicio y no puede ser solicitado.", ErrorType.Validation);
                }

                // Creamos el registro del detalle asociándolo al ID de nuestra solicitud.
                var purchaseRequestDetail = new PurchaseRequestDetail
                {
                    PurchaseRequestId = purchaseRequest.Id,
                    ProductId = detail.ProductId,
                    QuantityRequested = detail.QuantityRequested
                };
                
                _context.PurchaseRequestDetails.Add(purchaseRequestDetail);
            }

            // 6. Guardamos los detalles creados y finalizamos la transacción de forma exitosa.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 7. Retornamos la solicitud de compra recién creada buscándola por su ID.
            return await GetByIdAsync(purchaseRequest.Id);
        }
        catch (Exception ex)
        {
            // En caso de error inesperado, cancelamos la transacción y reportamos el fallo.
            await transaction.RollbackAsync();
            return Result<PurchaseRequestWrapperDto>.Failure($"{PurchaseRequestError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<ListPurchaseRequestsWrapperDto>> GetAllAsync()
    {
        // Consultamos todas las solicitudes de compra sin rastrear cambios (para mayor rapidez).
        // Las proyectamos directamente a DTOs de respuesta y las ordenamos por fecha (de más reciente a más antigua).
        var purchaseRequests = await _context.PurchaseRequests
            .AsNoTracking()
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(pr => pr.Date)
            .ToListAsync();

        return Result<ListPurchaseRequestsWrapperDto>.Success(new ListPurchaseRequestsWrapperDto { PurchaseRequests = purchaseRequests });
    }

    public async Task<Result<ListPurchaseRequestsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        // 1. Preparamos la consulta base sobre las solicitudes de compra.
        var query = _context.PurchaseRequests.AsNoTracking();

        // 2. Contamos la cantidad total de registros en la base de datos para la paginación.
        var totalElements = await query.CountAsync();

        // 3. Obtenemos el segmento correspondiente a la página solicitada y proyectamos a DTOs.
        var purchaseRequests = await query
            .OrderByDescending(pr => pr.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        // 4. Construimos el objeto de paginación con los metadatos necesarios.
        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListPurchaseRequestsWrapperDto>.Success(new ListPurchaseRequestsWrapperDto { PurchaseRequests = purchaseRequests, Pagination = _pagination });
    }

    public async Task<Result<PurchaseRequestWrapperDto>> GetByIdAsync(int id)
    {
        // Buscamos la solicitud de compra por su ID y la proyectamos al DTO de respuesta en una sola consulta.
        var purchaseRequest = await _context.PurchaseRequests
            .AsNoTracking()
            .Where(pr => pr.Id == id)
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        // Si no existe, retornamos un fallo indicando que no fue encontrada.
        if (purchaseRequest == null)
            return Result<PurchaseRequestWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<PurchaseRequestWrapperDto>.Success(new PurchaseRequestWrapperDto { PurchaseRequest = purchaseRequest });
    }
}
