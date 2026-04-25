using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.BillType;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.BillType;
using BackEnd.Models;

namespace BackEnd.Services;

public class BillTypeService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBillTypesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.BillTypes.AsNoTracking();

        var totalElements = await query.CountAsync();

        var billTypes = await query
            .OrderBy(bt => bt.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BillTypeResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBillTypesWrapperDto>.Success(new ListBillTypesWrapperDto { BillTypes = billTypes, Pagination = _pagination });
    }

    public async Task<Result<BillTypeWrapperDto>> GetByIdAsync(int id)
    {
        var billType = await _context.BillTypes
            .AsNoTracking()
            .Where(bt => bt.Id == id)
            .ProjectTo<BillTypeResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (billType == null)
            return Result<BillTypeWrapperDto>.Failure(BillTypeError.NotFound, ErrorType.NotFound);

        return Result<BillTypeWrapperDto>.Success(new BillTypeWrapperDto { BillType = billType });
    }

    public async Task<Result<BillTypeWrapperDto>> CreateAsync(CreateBillTypeRequestDto request)
    {
        var billType = new BillType
        {
            Name = request.Name
        };

        _context.BillTypes.Add(billType);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(billType.Id);
    }

    public async Task<Result<BillTypeWrapperDto>> UpdateAsync(int id, UpdateBillTypeRequestDto request)
    {
        var billType = await _context.BillTypes.FindAsync(id);

        if (billType == null)
            return Result<BillTypeWrapperDto>.Failure(BillTypeError.NotFound, ErrorType.NotFound);

        billType.Name = request.Name;

        _context.BillTypes.Update(billType);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(billType.Id);
    }

    // public async Task<Result> DeleteAsync(int id)
    // {
    //     var billType = await _context.BillTypes.FindAsync(id);

    //     if (billType == null)
    //         return Result.Failure(BillTypeError.NotFound, ErrorType.NotFound);

    //     _context.BillTypes.Remove(billType);
    //     await _context.SaveChangesAsync();

    //     return Result.Success();
    // }
}