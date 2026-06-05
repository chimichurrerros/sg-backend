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
using BackEnd.Services.Accounting;
using BackEnd.DTOs.Requests.Entry;

namespace BackEnd.Services;

public class BillService(AppDbContext context, IMapper mapper, EntryService entryService)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly EntryService _entryService = entryService;

    public async Task<Result<ListBillsWrapperDto>> GetListAsync(BillQueryDto queryDto)
    {
        var query = _context.Bills.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerName))
        {
            query = query.Where(b => b.Customer != null && b.Customer.Name.ToLower().Contains(queryDto.CustomerName.ToLower()));
        }

        if (queryDto.CustomerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == queryDto.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Number))
        {
            query = query.Where(b => b.Number.ToLower().Contains(queryDto.Number.ToLower()));
        }

        if (queryDto.Date.HasValue)
        {
            query = query.Where(b => b.Date == queryDto.Date.Value);
        }

        if (queryDto.StartDate.HasValue)
        {
            query = query.Where(b => b.Date >= queryDto.StartDate.Value);
        }

        if (queryDto.EndDate.HasValue)
        {
            query = query.Where(b => b.Date <= queryDto.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerRuc))
        {
            query = query.Where(b => b.Customer != null && b.Customer.Ruc.ToLower().Contains(queryDto.CustomerRuc.ToLower()));
        }

        if (queryDto.IsPurchaseBill.HasValue)
        {
            if (queryDto.IsPurchaseBill.Value)
                query = query.Where(b => b.PurchaseOrderForSupplierId != null);
            else
                query = query.Where(b => b.SalesOrderId != null);
        }

        if (queryDto.PurchaseOrderForSupplierId.HasValue)
        {
            query = query.Where(b => b.PurchaseOrderForSupplierId == queryDto.PurchaseOrderForSupplierId.Value);
        }

        var totalElements = await query.CountAsync();

        var bills = await query
            .Include(b => b.SalesOrder)
                .ThenInclude(so => so!.SalesOrderDetails)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        for (int i = 0; i < bills.Count; i++)
        {
            var salesOrder = bills[i].SalesOrder;
            if (salesOrder != null)
            {
                bills[i].Total = salesOrder.SalesOrderDetails.Sum(sod => sod.QuantityInvoiced * sod.Price);
            }
        }

        var billsDto = bills.Select(b => _mapper.Map<BillResponseDto>(b)).ToList();

        var _pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListBillsWrapperDto>.Success(new ListBillsWrapperDto { Bills = billsDto, Pagination = _pagination });
    }

    public async Task<Result<BillWrapperDto>> GetByIdAsync(int id)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .Include(b => b.SalesOrder)
                .ThenInclude(so => so!.SalesOrderDetails)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bill == null)
            return Result<BillWrapperDto>.Failure(BillError.NotFound, ErrorType.NotFound);

        if (bill.SalesOrder != null)
        {
            bill.Total = bill.SalesOrder.SalesOrderDetails.Sum(sod => sod.QuantityInvoiced * sod.Price);
        }

        var billDto = _mapper.Map<BillResponseDto>(bill);

        return Result<BillWrapperDto>.Success(new BillWrapperDto { Bill = billDto });
    }

    public async Task<Result<BillWrapperDto>> CreateAsync(CreateBillRequestDto request)
    {
        var bill = new Bill
        {
            BillType = request.BillType,
            BillState = request.BillState,
            CustomerId = request.CustomerId,
            SalesOrderId = request.SalesOrderId,
            PurchaseOrderForSupplierId = request.PurchaseOrderForSupplierId,
            Stamp = request.Stamp,
            Number = request.Number,
            Date = request.Date,
            DueDate = request.DueDate,
            PaymentTerms = request.PaymentTerms,
            Total = request.Total,
            TaxTotal = request.TaxTotal,
            IsCredit = request.IsCredit
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        if (bill.PurchaseOrderForSupplierId == null && bill.Total > 0)
        {

            var debitAccountMap = bill.BillType == BillTypeEnum.CONTADO
                ? AccountantPlanMap.Cajas
                : AccountantPlanMap.Cuentas;

            decimal tenPolcientoTotal = (bill.Total * 10) / 100;
            var entryDetails = new List<CreateEntryDetailDto>
            {
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)debitAccountMap,
                    Debit = bill.Total - tenPolcientoTotal,
                    Credit = 0m
                },
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)AccountantPlanMap.Ventas,
                    Debit = 0m,
                    Credit = bill.Total
                },
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)AccountantPlanMap.IVADebito,
                    Debit = tenPolcientoTotal,
                    Credit = 0m
                }
            };

            var entryResult = await _entryService.CreateAutomaticEntryAsync(
                new DateTime(bill.Date.Year, bill.Date.Month, bill.Date.Day, 12, 0, 0, DateTimeKind.Utc),
                $"Factura Emitida Nro. {bill.Number}",
                ModuleEnum.Sales,
                entryDetails
            );

            if (!entryResult.IsSuccess)
            {
                return Result<BillWrapperDto>.Failure($"Error al generar asiento automático: {entryResult.ErrorMessage}", entryResult.ErrorType);
            }
        }

        return await GetByIdAsync(bill.Id);
    }

    public async Task<Result<BillWrapperDto>> UpdateAsync(int id, UpdateBillRequestDto request)
    {
        var bill = await _context.Bills.FindAsync(id);

        if (bill == null)
            return Result<BillWrapperDto>.Failure(BillError.NotFound, ErrorType.NotFound);

        bill.BillType = request.BillType;
        bill.CustomerId = request.CustomerId;
        bill.SalesOrderId = request.SalesOrderId;
        bill.PurchaseOrderForSupplierId = request.PurchaseOrderForSupplierId;
        bill.Stamp = request.Stamp;
        bill.Number = request.Number;
        bill.Date = request.Date;
        bill.DueDate = request.DueDate;
        bill.PaymentTerms = request.PaymentTerms;
        bill.Total = request.Total;
        bill.TaxTotal = request.TaxTotal;
        bill.BillState = request.BillState;
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