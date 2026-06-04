using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.RequestForQuotation;
using BackEnd.DTOs.Responses.RequestForQuotation;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class RequestForQuotationService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListRequestForQuotationsWrapperDto>> GetAllAsync()
    {
        var rfqs = await LoadRfqQuery()
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        return Result<ListRequestForQuotationsWrapperDto>.Success(new ListRequestForQuotationsWrapperDto
        {
            RequestForQuotations = _mapper.Map<List<RequestForQuotationResponseDto>>(rfqs)
        });
    }

    public async Task<Result<ListRequestForQuotationsWrapperDto>> GetListAsync(RequestForQuotationQueryDto query)
    {
        var rQuery = LoadRfqQuery();

        if (query.SupplierId.HasValue)
            rQuery = rQuery.Where(r => r.SupplierId == query.SupplierId.Value);

        if (query.PurchaseRequestId.HasValue)
            rQuery = rQuery.Where(r => r.PurchaseRequestId == query.PurchaseRequestId.Value);

        if (query.State.HasValue)
            rQuery = rQuery.Where(r => (int)r.State == query.State.Value);

        var total = await rQuery.CountAsync();

        var rfqs = await rQuery
            .OrderByDescending(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<RequestForQuotationResponseDto>>(rfqs);

        return Result<ListRequestForQuotationsWrapperDto>.Success(new ListRequestForQuotationsWrapperDto
        {
            RequestForQuotations = dtos,
            Pagination = new Pagination(query.Page, query.PageSize, total)
        });
    }

    public async Task<Result<RequestForQuotationWrapperDto>> GetByIdAsync(int id)
    {
        var rfq = await LoadRfqQuery().FirstOrDefaultAsync(r => r.Id == id);

        if (rfq == null)
            return Result<RequestForQuotationWrapperDto>.Failure(RequestForQuotationError.NotFound, ErrorType.NotFound);

        return Result<RequestForQuotationWrapperDto>.Success(_mapper.Map<RequestForQuotationWrapperDto>(rfq));
    }

    private IQueryable<RequestForQuotation> LoadRfqQuery()
    {
        return _context.RequestForQuotations
            .AsNoTracking()
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseRequest)
            .Include(r => r.RequestForQuotationDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.ProductCategory);
    }
}
