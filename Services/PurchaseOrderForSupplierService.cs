using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PurchaseOrderForSupplierService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListPurchaseOrdersForSupplierWrapperDto>> GetAllAsync()
    {
        var orders = await LoadQuery()
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return Result<ListPurchaseOrdersForSupplierWrapperDto>.Success(new ListPurchaseOrdersForSupplierWrapperDto
        {
            PurchaseOrdersForSupplier = _mapper.Map<List<PurchaseOrderForSupplierResponseDto>>(orders)
        });
    }

    public async Task<Result<ListPurchaseOrdersForSupplierWrapperDto>> GetListAsync(PurchaseOrderForSupplierQueryDto query)
    {
        var rQuery = LoadQuery();

        if (query.BranchId.HasValue)
            rQuery = rQuery.Where(o => o.PurchaseOrder.BranchId == query.BranchId.Value);

        if (query.PurchaseOrderId.HasValue)
            rQuery = rQuery.Where(o => o.PurchaseOrderId == query.PurchaseOrderId.Value);

        if (query.SupplierId.HasValue)
            rQuery = rQuery.Where(o => o.SupplierId == query.SupplierId.Value);

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

        return Result<ListPurchaseOrdersForSupplierWrapperDto>.Success(new ListPurchaseOrdersForSupplierWrapperDto
        {
            PurchaseOrdersForSupplier = _mapper.Map<List<PurchaseOrderForSupplierResponseDto>>(orders),
            Pagination = new Pagination(query.Page, query.PageSize, total)
        });
    }

    public async Task<Result<PurchaseOrderForSupplierWrapperDto>> GetByIdAsync(int id)
    {
        var order = await LoadQuery().FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return Result<PurchaseOrderForSupplierWrapperDto>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        return Result<PurchaseOrderForSupplierWrapperDto>.Success(_mapper.Map<PurchaseOrderForSupplierWrapperDto>(order));
    }

    public async Task<Result<bool>> UpdateStateAsync(int id, UpdatePurchaseOrderForSupplierStateDto request)
    {
        var order = await _context.PurchaseOrdersForSupplier.FindAsync(id);
        if (order == null)
            return Result<bool>.Failure(PurchaseOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        if (!Enum.IsDefined(typeof(PurchaseOrderForSupplierStateEnum), request.State))
            return Result<bool>.Failure(PurchaseOrderError.InvalidState, ErrorType.Validation);

        order.State = (PurchaseOrderForSupplierStateEnum)request.State;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private IQueryable<PurchaseOrderForSupplier> LoadQuery()
    {
        return _context.PurchaseOrdersForSupplier
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.PurchaseOrder)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(o => o.PurchaseOrderDetails)
                .ThenInclude(d => d.SupplierQuoteDetail)
                    .ThenInclude(sd => sd!.SupplierQuote);
    }
}
