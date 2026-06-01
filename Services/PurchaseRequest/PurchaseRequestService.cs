using BackEnd.Constants.Errors;
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

        if (request.SupplierIds == null || request.SupplierIds.Count == 0)
            return Result<PurchaseRequestWrapperDto>.Failure(RequestForQuotationError.SuppliersRequired, ErrorType.Validation);

        if (request.SupplierIds.Count < 3)
            return Result<PurchaseRequestWrapperDto>.Failure(RequestForQuotationError.InsufficientSuppliers, ErrorType.Validation);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var productIds = request.Details.Select(d => d.ProductId).ToList();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var productMap = products.ToDictionary(p => p.Id);

            foreach (var detail in request.Details)
            {
                if (!productMap.TryGetValue(detail.ProductId, out var product))
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.ProductNotFound, ErrorType.Validation);
                }

                if (product.IsService == true)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure(PurchaseRequestError.ProductIsService, ErrorType.Validation);
                }
            }

            var validSuppliers = await _context.Suppliers
                .Include(s => s.SupplierCategories)
                .Where(s => request.SupplierIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            if (validSuppliers.Count != request.SupplierIds.Count)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseRequestWrapperDto>.Failure(RequestForQuotationError.InvalidSuppliers, ErrorType.Validation);
            }

            var productCategoryIds = products
                .Where(p => p.ProductCategoryId != null)
                .Select(p => p.ProductCategoryId!.Value)
                .Distinct()
                .ToHashSet();

            foreach (var supplier in validSuppliers)
            {
                var supplierCategoryIds = supplier.SupplierCategories
                    .Select(sc => sc.ProductCategoryId)
                    .ToHashSet();

                if (!supplierCategoryIds.Intersect(productCategoryIds).Any())
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseRequestWrapperDto>.Failure(RequestForQuotationError.SupplierNoCategoryMatch, ErrorType.Validation);
                }
            }

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
                var purchaseRequestDetail = new PurchaseRequestDetail
                {
                    PurchaseRequestId = purchaseRequest.Id,
                    ProductId = detail.ProductId,
                    QuantityRequested = detail.QuantityRequested
                };

                _context.PurchaseRequestDetails.Add(purchaseRequestDetail);
            }

            await _context.SaveChangesAsync();

            foreach (var supplier in validSuppliers)
            {
                var supplierCategoryIds = supplier.SupplierCategories
                    .Select(sc => sc.ProductCategoryId)
                    .ToHashSet();

                var matchingProductIds = products
                    .Where(p => p.ProductCategoryId != null && supplierCategoryIds.Contains(p.ProductCategoryId.Value))
                    .Select(p => p.Id)
                    .ToHashSet();

                var matchingDetails = request.Details
                    .Where(d => matchingProductIds.Contains(d.ProductId))
                    .ToList();

                if (matchingDetails.Count == 0)
                    continue;

                var rfq = new RequestForQuotation
                {
                    PurchaseRequestId = purchaseRequest.Id,
                    SupplierId = supplier.Id,
                    Date = DateTime.UtcNow,
                    Observation = request.Observation,
                    State = RequestForQuotationStateEnum.Pending
                };

                _context.RequestForQuotations.Add(rfq);
                await _context.SaveChangesAsync();

                foreach (var detail in matchingDetails)
                {
                    _context.RequestForQuotationDetails.Add(new RequestForQuotationDetail
                    {
                        RequestForQuotationId = rfq.Id,
                        ProductId = detail.ProductId,
                        QuantityRequested = detail.QuantityRequested
                    });
                }
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

    public async Task<Result<ListPurchaseRequestsWrapperDto>> GetListAsync(PurchaseRequestQueryDto query)
    {
        var rQuery = _context.PurchaseRequests.AsNoTracking();

        if (query.State.HasValue)
            rQuery = rQuery.Where(pr => (int)pr.PurchaseRequestState == query.State.Value);

        var totalElements = await rQuery.CountAsync();

        var purchaseRequests = await rQuery
            .OrderByDescending(pr => pr.Date)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<PurchaseRequestResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(query.Page, query.PageSize, totalElements);

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
