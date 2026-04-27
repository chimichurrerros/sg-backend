using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.BillDetail;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.BillDetail;
using BackEnd.Models;

namespace BackEnd.Services;

public class BillDetailService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBillDetailsWrapperDto>> GetListByBillIdAsync(int billId, PaginationRequestDto pagination)
    {
        var query = _context.BillDetails
            .AsNoTracking()
            .Where(bd => bd.BillId == billId);

        var totalElements = await query.CountAsync();

        var billDetails = await query
            .OrderBy(bd => bd.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BillDetailResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBillDetailsWrapperDto>.Success(new ListBillDetailsWrapperDto { BillDetails = billDetails, Pagination = _pagination });
    }

    public async Task<Result<BillDetailWrapperDto>> GetByIdAsync(int id)
    {
        var billDetail = await _context.BillDetails
            .AsNoTracking()
            .Where(bd => bd.Id == id)
            .ProjectTo<BillDetailResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (billDetail == null)
            return Result<BillDetailWrapperDto>.Failure(BillDetailError.NotFound, ErrorType.NotFound);

        return Result<BillDetailWrapperDto>.Success(new BillDetailWrapperDto { BillDetail = billDetail });
    }

    public async Task<Result<BillDetailWrapperDto>> CreateAsync(CreateBillDetailRequestDto request)
    {
        var billDetail = new BillDetail
        {
            BillId = request.BillId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Price = request.Price,
            TaxRate = request.TaxRate
        };

        _context.BillDetails.Add(billDetail);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(billDetail.Id);
    }

    public async Task<Result<BillDetailWrapperDto>> UpdateAsync(int id, UpdateBillDetailRequestDto request)
    {
        var billDetail = await _context.BillDetails.FindAsync(id);

        if (billDetail == null)
            return Result<BillDetailWrapperDto>.Failure(BillDetailError.NotFound, ErrorType.NotFound);

        billDetail.BillId = request.BillId;
        billDetail.ProductId = request.ProductId;
        billDetail.Quantity = request.Quantity;
        billDetail.Price = request.Price;
        billDetail.TaxRate = request.TaxRate;

        _context.BillDetails.Update(billDetail);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(billDetail.Id);
    }

    // public async Task<Result> DeleteAsync(int id)
    // {
    //     var billDetail = await _context.BillDetails.FindAsync(id);

    //     if (billDetail == null)
    //         return Result.Failure(BillDetailError.NotFound, ErrorType.NotFound);

    //     _context.BillDetails.Remove(billDetail);
    //     await _context.SaveChangesAsync();

    //     return Result.Success();
    // }
}