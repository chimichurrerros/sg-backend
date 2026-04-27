using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Bill;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Bill;
using BackEnd.Models;

namespace BackEnd.Services;

public class BillService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBillsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Bills.AsNoTracking();

        var totalElements = await query.CountAsync();

        var bills = await query
            .OrderBy(b => b.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BillResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBillsWrapperDto>.Success(new ListBillsWrapperDto { Bills = bills, Pagination = _pagination });
    }

    public async Task<Result<BillWrapperDto>> GetByIdAsync(int id)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .Where(b => b.Id == id)
            .ProjectTo<BillResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (bill == null)
            return Result<BillWrapperDto>.Failure(BillError.NotFound, ErrorType.NotFound);

        return Result<BillWrapperDto>.Success(new BillWrapperDto { Bill = bill });
    }

    public async Task<Result<BillWrapperDto>> CreateAsync(CreateBillRequestDto request)
    {
        var bill = new Bill
        {
            BillType = request.BillType,
            EntityId = request.EntityId,
            SalesOrderId = request.SalesOrderId,
            PurchaseOrderId = request.PurchaseOrderId,
            Stamp = request.Stamp,
            Number = request.Number,
            Date = request.Date,
            DueDate = request.DueDate,
            PaymentTerms = request.PaymentTerms,
            Total = request.Total,
            TaxTotal = request.TaxTotal,
            StateId = request.StateId,
            IsCredit = request.IsCredit
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(bill.Id);
    }

    public async Task<Result<BillWrapperDto>> UpdateAsync(int id, UpdateBillRequestDto request)
    {
        var bill = await _context.Bills.FindAsync(id);

        if (bill == null)
            return Result<BillWrapperDto>.Failure(BillError.NotFound, ErrorType.NotFound);

        bill.BillType = request.BillType;
        bill.EntityId = request.EntityId;
        bill.SalesOrderId = request.SalesOrderId;
        bill.PurchaseOrderId = request.PurchaseOrderId;
        bill.Stamp = request.Stamp;
        bill.Number = request.Number;
        bill.Date = request.Date;
        bill.DueDate = request.DueDate;
        bill.PaymentTerms = request.PaymentTerms;
        bill.Total = request.Total;
        bill.TaxTotal = request.TaxTotal;
        bill.StateId = request.StateId;
        bill.IsCredit = request.IsCredit;

        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(bill.Id);
    }

    // public async Task<Result> DeleteAsync(int id)
    // {
    //     var bill = await _context.Bills.FindAsync(id);

    //     if (bill == null)
    //         return Result.Failure(BillError.NotFound, ErrorType.NotFound);

    //     _context.Bills.Remove(bill);
    //     await _context.SaveChangesAsync();

    //     return Result.Success();
    // }
}