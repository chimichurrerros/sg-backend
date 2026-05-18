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
        if (request.Details == null || request.Details.Count == 0)
            return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.DetailsRequired, ErrorType.Validation);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var purchaseRequest = new PurchaseRequest
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                PurchaseRequestState = PurchaseRequestStateEnum.Pending,
                Observation = request.Observation
            };

            _context.PurchaseRequests.Add(purchaseRequest);
            await _context.SaveChangesAsync();

            foreach (var detail in request.Details)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == detail.ProductId);
                if (product == null)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.ProductNotFound, ErrorType.Validation);
                }

                if (product.IsService == true)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure($"El producto '{product.Name}' es un servicio y no puede ser solicitado.", ErrorType.Validation);
                }

                var purchaseRequestDetail = new PurchaseRequestDetail
                {
                    PurchaseRequestId = purchaseRequest.Id,
                    ProductId = detail.ProductId,
                    QuantityRequested = detail.QuantityRequested
                };
                
                _context.PurchaseRequestDetails.Add(purchaseRequestDetail);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(purchaseRequest.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<PurchaseRequestWrapperDto>.Failure($"{PurchaseRequestError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<ListPurchaseRequestsWrapperDto>> GetAllAsync()
    {
        var purchaseRequests = await _context.PurchaseRequests
            .AsNoTracking()
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(pr => pr.Date)
            .ToListAsync();

        return Result<ListPurchaseRequestsWrapperDto>.Success(new ListPurchaseRequestsWrapperDto { PurchaseRequests = purchaseRequests });
    }

    public async Task<Result<ListPurchaseRequestsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.PurchaseRequests.AsNoTracking();

        var totalElements = await query.CountAsync();

        var purchaseRequests = await query
            .OrderByDescending(pr => pr.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListPurchaseRequestsWrapperDto>.Success(new ListPurchaseRequestsWrapperDto { PurchaseRequests = purchaseRequests, Pagination = _pagination });
    }

    public async Task<Result<PurchaseRequestWrapperDto>> GetByIdAsync(int id)
    {
        var purchaseRequest = await _context.PurchaseRequests
            .AsNoTracking()
            .Where(pr => pr.Id == id)
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (purchaseRequest == null)
            return Result<PurchaseRequestWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<PurchaseRequestWrapperDto>.Success(new PurchaseRequestWrapperDto { PurchaseRequest = purchaseRequest });
    }
}
