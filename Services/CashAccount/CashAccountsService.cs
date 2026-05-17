using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.CashAccount;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.CashAccount;
using BackEnd.Models;

namespace BackEnd.Services;

public class CashAccountsService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListCashAccountsWrapperDto>> GetAllAsync()
    {
        var cashAccounts = await _context.CashAccounts
            .AsNoTracking()
            .ProjectTo<CashAccountResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListCashAccountsWrapperDto>.Success(new ListCashAccountsWrapperDto { CashAccounts = cashAccounts });
    }

    public async Task<Result<ListCashAccountsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.CashAccounts.AsNoTracking();

        var totalElements = await query.CountAsync();

        var cashAccounts = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<CashAccountResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListCashAccountsWrapperDto>.Success(new ListCashAccountsWrapperDto { CashAccounts = cashAccounts, Pagination = _pagination });
    }

    public async Task<Result<CashAccountWrapperDto>> GetByIdAsync(int id)
    {
        var cashAccount = await _context.CashAccounts
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<CashAccountResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (cashAccount == null)
            return Result<CashAccountWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<CashAccountWrapperDto>.Success(new CashAccountWrapperDto { CashAccount = cashAccount });
    }

    public async Task<Result<CashAccountWrapperDto>> CreateAsync(CashAccountRequestDto request)
    {
        var cashAccount = _mapper.Map<CashAccount>(request);
        cashAccount.Balance = request.InitialBalance;

        _context.CashAccounts.Add(cashAccount);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(cashAccount.Id);
    }

    public async Task<Result<CashAccountWrapperDto>> UpdateAsync(int id, CashAccountRequestDto request)
    {
        var cashAccount = await _context.CashAccounts.FindAsync(id);

        if (cashAccount == null)
            return Result<CashAccountWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, cashAccount);

        _context.CashAccounts.Update(cashAccount);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(cashAccount.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var cashAccount = await _context.CashAccounts.FindAsync(id);

        if (cashAccount == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        cashAccount.IsDeleted = true;
        _context.CashAccounts.Update(cashAccount);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
