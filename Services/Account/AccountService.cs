using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Accounts;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Accounts;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class AccountService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListAccountsWrapperDto>> GetAllAsync()
    {
        var accounts = await _context.Accounts
            .AsNoTracking()
            .ProjectTo<AccountResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListAccountsWrapperDto>.Success(new ListAccountsWrapperDto { Accounts = accounts });
    }

    public async Task<Result<ListAccountsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Accounts.AsNoTracking();

        var totalElements = await query.CountAsync();

        var accounts = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<AccountResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListAccountsWrapperDto>.Success(new ListAccountsWrapperDto { Accounts = accounts, Pagination = _pagination });
    }

    public async Task<Result<AccountWrapperDto>> GetByIdAsync(int id)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<AccountResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (account == null)
            return Result<AccountWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<AccountWrapperDto>.Success(new AccountWrapperDto { Account = account });
    }

    public async Task<Result<AccountWrapperDto>> CreateAsync(CreateAccountRequestDto request)
    {
        var newAccount = _mapper.Map<Account>(request);

        // REGLA DE NEGOCIO: Una cuenta nueva siempre nace con saldo 0
        newAccount.CurrentBalance = 0;
        newAccount.AvailableBalance = 0;

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(newAccount.Id);
    }

    public async Task<Result<AccountWrapperDto>> UpdateAsync(int id, UpdateAccountRequestDto request)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
            return Result<AccountWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        // Mapeamos los datos permitidos
        _mapper.Map(request, account);

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(account.Id);
    }

    // public async Task<Result> DeleteAsync(int id)
    // {
    //     var account = await _context.Accounts.FindAsync(id);

    //     if (account == null)
    //         return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

    //     // REGLA DE NEGOCIO: No se puede borrar una cuenta si ya tiene movimientos
    //     var hasMovements = await _context.BankMovements.AnyAsync(bm => bm.AccountId == id);
    //     if (hasMovements)
    //         return Result.Failure("No se puede eliminar la cuenta porque ya tiene movimientos registrados.", ErrorType.Validation);

    //     _context.Accounts.Remove(account);
    //     await _context.SaveChangesAsync();

    //     return Result.Success();
    // }

    public async Task<Result> ToggleStatusAsync(int id)
	{
		var account = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == id);

		if (account == null)
			return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

		account.IsActive = !account.IsActive;

		_context.Accounts.Update(account);
		await _context.SaveChangesAsync();

		return Result.Success();
	}
}