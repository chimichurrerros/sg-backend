using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.AccountPlan;
using BackEnd.DTOs.Responses.AccountPlan;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Services;

public class AccountPlanService(AppDbContext context, IMapper mapper, AccountantProcessService accountantProcessService)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly AccountantProcessService _accountantProcessService = accountantProcessService;

    public async Task<Result<ListAccountPlansWrapperDto>> GetAllAsync()
    {
        var ap = await _context.AccountPlans
            .AsNoTracking()
            .ProjectTo<AccountPlanResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListAccountPlansWrapperDto>.Success(new ListAccountPlansWrapperDto { AccountPlans = ap });
    }

    public async Task<Result<ListAccountPlansWrapperDto>> GetListAsync(PaginationRequestDto request)
    {
        var ap = _context.AccountPlans.AsNoTracking();

        var totalElements = await ap.CountAsync();

        var result = await ap
            .OrderBy(v => v.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<AccountPlanResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(request.Page, request.PageSize, totalElements);

        return Result<ListAccountPlansWrapperDto>.Success(new ListAccountPlansWrapperDto { AccountPlans = result, Pagination = _pagination });
    }

    public async Task<Result<AccountPlanWrapperDto>> GetByIdAsync(int id)
    {
        var ap = await _context.AccountPlans
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<AccountPlanResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (ap == null)
            return Result<AccountPlanWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<AccountPlanWrapperDto>.Success(new AccountPlanWrapperDto { AccountPlan = ap });
    }

    public async Task<Result<AccountPlanWrapperDto>> CreateAsync(CreateAccountPlanRequestDto request)
    {
        if (!await _accountantProcessService.IsProcessActiveAsync(request.AccountantProcessId))
            return Result<AccountPlanWrapperDto>.Failure(AccountingError.ProcessExpiredOrNotExists, ErrorType.Validation);

        var ap = _mapper.Map<AccountPlan>(request);

        _context.AccountPlans.Add(ap);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(ap.Id);
    }

    public async Task<Result<AccountPlanWrapperDto>> UpdateAsync(int id, UpdateAccountPlanRequestDto request)
    {
        var ap = await _context.AccountPlans.FindAsync(id);

        if (ap == null)
            return Result<AccountPlanWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        // Check if the current period is valid (can't modify records of closed periods)
        if (!await _accountantProcessService.IsProcessActiveAsync(ap.AccountantProcessId))
            return Result<AccountPlanWrapperDto>.Failure(AccountingError.CurrentProcessExpired, ErrorType.Validation);

        // If changing to a different process, check if the new period is valid
        if (ap.AccountantProcessId != request.AccountantProcessId && !await _accountantProcessService.IsProcessActiveAsync(request.AccountantProcessId))
            return Result<AccountPlanWrapperDto>.Failure(AccountingError.NewProcessExpiredOrNotExists, ErrorType.Validation);

        _mapper.Map(request, ap);
        _context.AccountPlans.Update(ap);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(ap.Id);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var ap = await _context.AccountPlans.FindAsync(id);

        if (ap == null)
            return Result<bool>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        if (!await _accountantProcessService.IsProcessActiveAsync(ap.AccountantProcessId))
            return Result<bool>.Failure(AccountingError.CannotDeleteProcessExpired, ErrorType.Validation);

        _context.AccountPlans.Remove(ap);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
