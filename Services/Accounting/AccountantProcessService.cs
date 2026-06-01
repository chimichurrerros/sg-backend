using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.AccountantProcess;
using BackEnd.DTOs.Responses.AccountantProcess;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Services;

public class AccountantProcessService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListAccountantProcessesWrapperDto>> GetAllAsync()
    {
        var ap = await _context.AccountantProcesses
            .AsNoTracking()
            .ProjectTo<AccountantProcessResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListAccountantProcessesWrapperDto>.Success(new ListAccountantProcessesWrapperDto { AccountantProcesses = ap });
    }

    public async Task<Result<ListAccountantProcessesWrapperDto>> GetListAsync(PaginationRequestDto request)
    {
        var ap = _context.AccountantProcesses.AsNoTracking();

        var totalElements = await ap.CountAsync();

        var result = await ap
            .OrderBy(v => v.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<AccountantProcessResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(request.Page, request.PageSize, totalElements);

        return Result<ListAccountantProcessesWrapperDto>.Success(new ListAccountantProcessesWrapperDto { AccountantProcesses = result, Pagination = _pagination });
    }

    public async Task<Result<AccountantProcessWrapperDto>> GetByIdAsync(int id)
    {
        var ap = await _context.AccountantProcesses
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<AccountantProcessResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (ap == null)
            return Result<AccountantProcessWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<AccountantProcessWrapperDto>.Success(new AccountantProcessWrapperDto { AccountantProcess = ap });
    }

    public async Task<Result<AccountantProcessWrapperDto>> GetLastAsync()
    {
        var ap = await _context.AccountantProcesses
            .AsNoTracking()
            .OrderByDescending(u => u.Id)
            .ProjectTo<AccountantProcessResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (ap == null)
            return Result<AccountantProcessWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<AccountantProcessWrapperDto>.Success(new AccountantProcessWrapperDto { AccountantProcess = ap });
    }

    public async Task<bool> IsProcessActiveAsync(int processId)
    {
        var process = await _context.AccountantProcesses.FindAsync(processId);
        if (process == null) return false;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return !process.IsClosed && process.EndDate >= today;
    }

    public async Task<Result<AccountantProcessWrapperDto>> CreateAsync(CreateAccountantProcessRequestDto request)
    {
        var ap = _mapper.Map<AccountantProcess>(request);

        _context.AccountantProcesses.Add(ap);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(ap.Id);
    }

    public async Task<Result<AccountantProcessWrapperDto>> UpdateAsync(int id, UpdateAccountantProcessRequestDto request)
    {
        var ap = await _context.AccountantProcesses.FindAsync(id);

        if (ap == null)
            return Result<AccountantProcessWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, ap);
        _context.AccountantProcesses.Update(ap);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(ap.Id);
    }
}
