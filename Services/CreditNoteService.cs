using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.CreditNote;
using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Services.Accounting;
using BackEnd.DTOs.Requests.Entry;

namespace BackEnd.Services;

public class CreditNoteService(AppDbContext context, IMapper mapper, EntryService entryService)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly EntryService _entryService = entryService;

    public async Task<Result<ListCreditNotesWrapperDto>> GetListAsync(CreditNoteQueryDto queryDto)
    {
        var query = _context.CreditNotes.AsNoTracking();

        if (queryDto.Type.HasValue)
            query = query.Where(cn => cn.Type == queryDto.Type.Value);

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerName))
            query = query.Where(cn => cn.Bill.Customer != null && cn.Bill.Customer.Name.ToLower().Contains(queryDto.CustomerName.ToLower()));

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerRuc))
            query = query.Where(cn => cn.Bill.Customer != null && cn.Bill.Customer.Ruc.ToLower().Contains(queryDto.CustomerRuc.ToLower()));

        if (!string.IsNullOrWhiteSpace(queryDto.BillNumber))
            query = query.Where(cn => cn.Bill.Number.ToLower().Contains(queryDto.BillNumber.ToLower()));

        if (!string.IsNullOrWhiteSpace(queryDto.Reason))
            query = query.Where(cn => cn.Reason.ToLower().Contains(queryDto.Reason.ToLower()));

        if (queryDto.Date.HasValue)
            query = query.Where(cn => cn.Date.Date == queryDto.Date.Value.Date);

        if (queryDto.MinDate.HasValue)
            query = query.Where(cn => cn.Date >= queryDto.MinDate.Value);

        if (queryDto.MaxDate.HasValue)
            query = query.Where(cn => cn.Date <= queryDto.MaxDate.Value);

        var totalElements = await query.CountAsync();

        var list = await query
            .OrderByDescending(cn => cn.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<CreditNoteResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListCreditNotesWrapperDto>.Success(new ListCreditNotesWrapperDto { CreditNotes = list, Pagination = pagination });
    }

    public async Task<Result<CreditNoteWrapperDto>> CreateAsync(CreateCreditNoteDto request)
    {
        if (request.BillId <= 0)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.BillIdRequired, ErrorType.Validation);

        var bill = await _context.Bills.Include(b => b.BillDetails).Include(b => b.Customer).FirstOrDefaultAsync(b => b.Id == request.BillId);
        if (bill == null)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.BillNotFound, ErrorType.NotFound);

        if (request.Details == null || request.Details.Count == 0)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.DetailsRequired, ErrorType.Validation);

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

            foreach (var d in request.Details)
            {
                _context.CreditNoteDetails.Add(new CreditNoteDetail
                {
                    CreditNoteId = creditNote.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                });
            }

            await _context.SaveChangesAsync();

            if (creditNote.Total > 0)
            {
                var dateOnly = DateOnly.FromDateTime(creditNote.Date);
                var activeProcess = await _context.AccountantProcesses
                    .FirstOrDefaultAsync(ap => !ap.IsClosed && ap.StartDate <= dateOnly && ap.EndDate >= dateOnly);

                if (activeProcess == null)
                {
                    await transaction.RollbackAsync();
                    return Result<CreditNoteWrapperDto>.Failure($"No existe un período contable activo para la fecha {creditNote.Date:dd/MM/yyyy}.", ErrorType.Validation);
                }

                // Debit Account: Ventas
                var debitAccount = await _context.AccountPlans
                    .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Order == (int)AccountantPlanMap.Ventas);

                // Credit Account: Cajas (if Contado) or Cuentas (if Credito)
                var creditAccountMap = bill.BillType == BillTypeEnum.CONTADO 
                    ? AccountantPlanMap.Cajas 
                    : AccountantPlanMap.Cuentas;

                var creditAccount = await _context.AccountPlans
                    .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Order == (int)creditAccountMap);

                if (debitAccount == null)
                {
                    await transaction.RollbackAsync();
                    return Result<CreditNoteWrapperDto>.Failure("No se encontró la cuenta contable 'Ventas' en el período contable activo.", ErrorType.Validation);
                }

                if (creditAccount == null)
                {
                    await transaction.RollbackAsync();
                    return Result<CreditNoteWrapperDto>.Failure($"No se encontró la cuenta contable '{creditAccountMap}' en el período contable activo.", ErrorType.Validation);
                }

                var entryDetails = new List<CreateEntryDetailDto>
                {
                    new CreateEntryDetailDto
                    {
                        AccountPlanId = debitAccount.Id,
                        Debit = creditNote.Total,
                        Credit = 0m
                    },
                    new CreateEntryDetailDto
                    {
                        AccountPlanId = creditAccount.Id,
                        Debit = 0m,
                        Credit = creditNote.Total
                    }
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
                    return Result<CreditNoteWrapperDto>.Failure($"Error al generar asiento automático: {entryResult.ErrorMessage}", entryResult.ErrorType);
                }
            }

            await transaction.CommitAsync();

            // Map response
            var response = new CreditNoteResponseDto
            {
                Id = creditNote.Id,
                BillId = creditNote.BillId,
                BillNumber = bill.Number,
                Type = creditNote.Type,
                CustomerId = bill.Customer?.Id ?? 0,
                CustomerName = bill.Customer?.Name ?? string.Empty,
                CustomerRuc = bill.Customer?.Ruc ?? string.Empty,
                Date = creditNote.Date,
                Total = creditNote.Total,
                Reason = creditNote.Reason,
                Details = creditNote.CreditNoteDetails.Select(d => new CreditNoteDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            };

            return Result<CreditNoteWrapperDto>.Success(new CreditNoteWrapperDto { CreditNote = response });
        }
        catch (Exception ex)
        {
            await _context.Database.RollbackTransactionAsync();
            return Result<CreditNoteWrapperDto>.Failure($"{CreditNoteError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<CreditNoteWrapperDto>> GetByIdAsync(int id)
    {
        var cn = await _context.CreditNotes
            .Include(c => c.CreditNoteDetails)
                .ThenInclude(d => d.Product)
            .Include(c => c.Bill)
                .ThenInclude(b => b.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cn == null)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.CreditNoteNotFound, ErrorType.NotFound);

        var response = new CreditNoteResponseDto
        {
            Id = cn.Id,
            BillId = cn.BillId,
            BillNumber = cn.Bill?.Number ?? string.Empty,
            Type = cn.Type,
            CustomerId = cn.Bill?.Customer?.Id ?? 0,
            CustomerName = cn.Bill?.Customer?.Name ?? string.Empty,
            CustomerRuc = cn.Bill?.Customer?.Ruc ?? string.Empty,
            Date = cn.Date,
            Total = cn.Total,
            Reason = cn.Reason,
            Details = cn.CreditNoteDetails.Select(d => new CreditNoteDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                Quantity = d.Quantity,
                Price = d.Price
            }).ToList()
        };

        return Result<CreditNoteWrapperDto>.Success(new CreditNoteWrapperDto { CreditNote = response });
    }
}
