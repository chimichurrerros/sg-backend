using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.SalesReturn;
using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.DTOs.Responses.SalesReturn;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Services.Accounting;
using BackEnd.DTOs.Requests.Entry;

namespace BackEnd.Services;

public class SalesReturnService(AppDbContext context, StockService stockService, IMapper mapper, EntryService entryService, BillNumberService billNumberService)
{
    private readonly AppDbContext _context = context;
    private readonly StockService _stockService = stockService;
    private readonly IMapper _mapper = mapper;
    private readonly EntryService _entryService = entryService;
    private readonly BillNumberService _billNumberService = billNumberService;

    public async Task<Result<SalesReturnWrapperDto>> CreateAsync(CreateSalesReturnDto request)
    {
        if (request.BillId <= 0)
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.BillNotFound, ErrorType.Validation);

        if (request.Details == null || request.Details.Count == 0)
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.DetailsRequired, ErrorType.Validation);

        var bill = await _context.Bills
            .Include(b => b.Customer)
            .Include(b => b.SalesOrder)
                .ThenInclude(so => so!.SalesOrderDetails)
            .Include(b => b.SalesOrder)
                .ThenInclude(so => so!.Branch)
            .FirstOrDefaultAsync(b => b.Id == request.BillId);

        if (bill == null)
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.BillNotFound, ErrorType.NotFound);

        var salesOrder = bill.SalesOrder;
        if (salesOrder == null)
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.SalesOrderNotFound, ErrorType.NotFound);

        var elapsed = DateTime.UtcNow - salesOrder.Date;
        if (elapsed > TimeSpan.FromHours(48))
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.ReturnPeriodExpired, ErrorType.Validation);

        foreach (var detail in request.Details)
        {
            if (detail.Quantity <= 0)
                return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.QuantityExceedsSold, ErrorType.Validation);

            var sod = salesOrder.SalesOrderDetails
                .FirstOrDefault(d => d.ProductId == detail.ProductId);

            if (sod == null)
                return Result<SalesReturnWrapperDto>.Failure(
                    $"{SalesReturnError.ProductNotInSale} (Producto ID: {detail.ProductId})", ErrorType.Validation);

            if (detail.Quantity > sod.QuantityInvoiced)
                return Result<SalesReturnWrapperDto>.Failure(
                    $"{SalesReturnError.QuantityExceedsSold} (Producto ID: {detail.ProductId}, Facturado: {sod.QuantityInvoiced})", ErrorType.Validation);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var creditNote = new CreditNote
            {
                BillId = request.BillId,
                Type = CreditNoteTypeEnum.SalesReturn,
                Date = request.Date == default ? DateTime.UtcNow : request.Date,
                Total = request.Total,
                Reason = request.Reason
            };

            _context.CreditNotes.Add(creditNote);
            await _context.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(request.Number))
                creditNote.Number = await _billNumberService.GetNextCreditNoteNumber(salesOrder.BranchId);
            else
                creditNote.Number = request.Number;

            foreach (var detail in request.Details)
            {
                _context.CreditNoteDetails.Add(new CreditNoteDetail
                {
                    CreditNoteId = creditNote.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price
                });

                var stockResult = await _stockService.IncreaseStockAsync(
                    detail.ProductId, salesOrder.BranchId, detail.Quantity);

                if (!stockResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<SalesReturnWrapperDto>.Failure(stockResult.ErrorMessage!, stockResult.ErrorType);
                }
            }

            await _context.SaveChangesAsync();

            var customerName = bill.Customer?.Name ?? string.Empty;
            var customerRuc = bill.Customer?.Ruc ?? string.Empty;

            var salesReturn = new Models.SalesReturn
            {
                CreditNoteId = creditNote.Id,
                BillId = request.BillId,
                SalesOrderId = salesOrder.Id,
                SalesOrderNumber = salesOrder.Number,
                BranchId = salesOrder.BranchId,
                CustomerId = bill.CustomerId ?? 0,
                CustomerName = customerName,
                CustomerRuc = customerRuc,
                Date = creditNote.Date,
                Total = creditNote.Total,
                Reason = creditNote.Reason
            };

            _context.SalesReturns.Add(salesReturn);
            await _context.SaveChangesAsync();

            if (true)
            {

                // Credit Account: Cajas (if Contado) or Cuentas (if Credito)
                var creditAccountMap = bill.BillType == BillTypeEnum.CONTADO 
                    ? AccountantPlanMap.Cajas 
                    : AccountantPlanMap.Cuentas;

                decimal tenPolcientoTotal = (creditNote.Total * 10) / 100;
                var entryDetails = new List<CreateEntryDetailDto>
                {
                    new CreateEntryDetailDto
                    {
                        AccountPlanId = (int)AccountantPlanMap.Ventas,
                        Debit = creditNote.Total - tenPolcientoTotal,
                        Credit = 0m
                    },

                    new CreateEntryDetailDto
                    {
                        AccountPlanId = (int)AccountantPlanMap.IVADebito,
                        Debit = tenPolcientoTotal,
                        Credit = 0m
                    },

                    new CreateEntryDetailDto
                    {
                        AccountPlanId = (int)creditAccountMap,
                        Debit = 0m,
                        Credit = creditNote.Total
                    },

  
                };

                var entryResult = await _entryService.CreateAutomaticEntryAsync(
                    creditNote.Date,
                    $"Nota de Crédito Emitida Nro. {creditNote.Number ?? creditNote.Id.ToString()}",
                    ModuleEnum.Sales,
                    entryDetails
                );

                if (!entryResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<SalesReturnWrapperDto>.Failure($"Error al generar asiento automático: {entryResult.ErrorMessage}", entryResult.ErrorType);
                }
            }

            await transaction.CommitAsync();

            return await GetByIdAsync(salesReturn.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<SalesReturnWrapperDto>.Failure($"{SalesReturnError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<SalesReturnWrapperDto>> GetByIdAsync(int id)
    {
        var salesReturn = await _context.SalesReturns
            .AsNoTracking()
            .Include(sr => sr.Branch)
            .Include(sr => sr.CreditNote)
                .ThenInclude(cn => cn!.CreditNoteDetails)
                    .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (salesReturn == null)
            return Result<SalesReturnWrapperDto>.Failure(SalesReturnError.NotFound, ErrorType.NotFound);

        var response = _mapper.Map<SalesReturnResponseDto>(salesReturn);
        return Result<SalesReturnWrapperDto>.Success(new SalesReturnWrapperDto { SalesReturn = response });
    }

    public async Task<Result<ListSalesReturnsWrapperDto>> GetListAsync(SalesReturnQueryDto queryDto)
    {
        IQueryable<Models.SalesReturn> query = _context.SalesReturns
            .AsNoTracking()
            .Include(sr => sr.Branch)
            .Include(sr => sr.CreditNote)
                .ThenInclude(cn => cn!.CreditNoteDetails)
                    .ThenInclude(d => d.Product);

        if (!string.IsNullOrWhiteSpace(queryDto.SalesOrderNumber))
            query = query.Where(sr => sr.SalesOrderNumber.ToLower().Contains(queryDto.SalesOrderNumber.ToLower()));

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerName))
            query = query.Where(sr => sr.CustomerName.ToLower().Contains(queryDto.CustomerName.ToLower()));

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerRuc))
            query = query.Where(sr => sr.CustomerRuc.ToLower().Contains(queryDto.CustomerRuc.ToLower()));

        if (queryDto.BranchId.HasValue)
            query = query.Where(sr => sr.BranchId == queryDto.BranchId.Value);

        if (queryDto.Date.HasValue)
            query = query.Where(sr => sr.Date.Date == queryDto.Date.Value.Date);

        if (queryDto.MinDate.HasValue)
            query = query.Where(sr => sr.Date >= queryDto.MinDate.Value);

        if (queryDto.MaxDate.HasValue)
            query = query.Where(sr => sr.Date <= queryDto.MaxDate.Value);

        var totalElements = await query.CountAsync();

        var list = await query
            .OrderByDescending(sr => sr.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<SalesReturnResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListSalesReturnsWrapperDto>.Success(
            new ListSalesReturnsWrapperDto { SalesReturns = list, Pagination = _pagination });
    }

    public async Task<Result<ListSalesReturnsWrapperDto>> GetAllAsync()
    {
        var list = await _context.SalesReturns
            .AsNoTracking()
            .Include(sr => sr.Branch)
            .Include(sr => sr.CreditNote)
                .ThenInclude(cn => cn!.CreditNoteDetails)
                    .ThenInclude(d => d.Product)
            .OrderByDescending(sr => sr.Id)
            .ProjectTo<SalesReturnResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListSalesReturnsWrapperDto>.Success(new ListSalesReturnsWrapperDto { SalesReturns = list });
    }
}
