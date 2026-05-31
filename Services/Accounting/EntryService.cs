using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Entry;
using BackEnd.DTOs.Responses.Entry;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Services;

public class EntryService(AppDbContext context, IMapper mapper, AccountantProcessService accountantProcessService)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly AccountantProcessService _accountantProcessService = accountantProcessService;

    public async Task<Result<ListEntriesWrapperDto>> GetAllAsync()
    {
        var entries = await _context.Entries
            .AsNoTracking()
            .ProjectTo<EntryResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListEntriesWrapperDto>.Success(new ListEntriesWrapperDto { Entries = entries });
    }

    public async Task<Result<ListEntriesWrapperDto>> GetListAsync(PaginationRequestDto request)
    {
        var query = _context.Entries.AsNoTracking();

        var totalElements = await query.CountAsync();

        var result = await query
            .OrderBy(v => v.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<EntryResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(request.Page, request.PageSize, totalElements);

        return Result<ListEntriesWrapperDto>.Success(new ListEntriesWrapperDto { Entries = result, Pagination = _pagination });
    }

    public async Task<Result<EntryWrapperDto>> GetByIdAsync(int id)
    {
        var entry = await _context.Entries
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<EntryResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (entry == null)
            return Result<EntryWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<EntryWrapperDto>.Success(new EntryWrapperDto { Entry = entry });
    }

    public async Task<Result<EntryWrapperDto>> CreateAsync(CreateEntryRequestDto request)
    {
        if (!await _accountantProcessService.IsProcessActiveAsync(request.AccountantProcessId))
            return Result<EntryWrapperDto>.Failure(AccountingError.ProcessExpiredOrNotExists, ErrorType.Validation);

        var entry = _mapper.Map<Entry>(request);

        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entry.Id);
    }

    public async Task<Result<EntryWrapperDto>> UpdateAsync(int id, UpdateEntryRequestDto request)
    {
        var entry = await _context.Entries
            .Include(e => e.EntryDetails)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry == null)
            return Result<EntryWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        if (!await _accountantProcessService.IsProcessActiveAsync(entry.AccountantProcessId))
            return Result<EntryWrapperDto>.Failure(AccountingError.CurrentProcessExpired, ErrorType.Validation);

        if (entry.AccountantProcessId != request.AccountantProcessId && !await _accountantProcessService.IsProcessActiveAsync(request.AccountantProcessId))
            return Result<EntryWrapperDto>.Failure(AccountingError.NewProcessExpiredOrNotExists, ErrorType.Validation);

        _mapper.Map(request, entry);
        _context.Entries.Update(entry);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entry.Id);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var entry = await _context.Entries.FindAsync(id);

        if (entry == null)
            return Result<bool>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        if (!await _accountantProcessService.IsProcessActiveAsync(entry.AccountantProcessId))
            return Result<bool>.Failure(AccountingError.CannotDeleteProcessExpired, ErrorType.Validation);

        _context.Entries.Remove(entry);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<Entry>> CreateAutomaticEntryAsync(
        DateTime date, 
        string description, 
        ModuleEnum module, 
        List<CreateEntryDetailDto> details)
    {
        if (details == null || !details.Any())
        {
            return Result<Entry>.Failure("El asiento contable debe tener al menos un detalle.", ErrorType.Validation);
        }

        // Validar partida doble
        var totalDebit = details.Sum(d => d.Debit);
        var totalCredit = details.Sum(d => d.Credit);
        if (totalDebit != totalCredit)
        {
            return Result<Entry>.Failure($"El asiento contable no está balanceado. Debe ({totalDebit}) != Haber ({totalCredit}).", ErrorType.Validation);
        }

        // Buscar proceso contable activo para la fecha
        var dateOnly = DateOnly.FromDateTime(date);
        var activeProcess = await _context.AccountantProcesses
            .FirstOrDefaultAsync(ap => ap.StartDate <= dateOnly && ap.EndDate >= dateOnly);

        if (activeProcess == null)
        {
            return Result<Entry>.Failure($"No existe un período contable activo para la fecha {date:dd/MM/yyyy}.", ErrorType.Validation);
        }

        var entry = new Entry
        {
            Date = date,
            Description = description,
            Module = module,
            AccountantProcessId = activeProcess.Id,
            EntryDetails = details.Select(d => new EntryDetail
            {
                AccountPlanId = d.AccountPlanId,
                Debit = d.Debit,
                Credit = d.Credit
            }).ToList()
        };

        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        return Result<Entry>.Success(entry);
    }
}

