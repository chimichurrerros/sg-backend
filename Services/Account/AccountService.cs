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
using BackEnd.Constants.Errors;

namespace BackEnd.Services;

public class AccountService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

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
        // Usamos Include para traer los datos del banco si es que tiene uno asociado
        var accounts = await _context.Accounts
            .Include(a => a.Bank)
            .Where(a => a.IsActive)
            .ToListAsync();

        var response = _mapper.Map<IEnumerable<AccountResponseDto>>(accounts);
        return Result<IEnumerable<AccountResponseDto>>.Success(response);
    }

    public async Task<Result<AccountWrapperDto>> GetByIdAsync(int id)
    {
        var account = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        if (account == null)
            return Result<AccountWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<AccountWrapperDto>.Success(new AccountWrapperDto { Account = account });
    }

    public async Task<Result<AccountWrapperDto>> CreateAsync(CreateAccountRequestDto request)
    {
        var newAccount = _mapper.Map<Account>(request);

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(newAccount.Id);
    }

    public async Task<Result<AccountWrapperDto>> UpdateAsync(int id, UpdateAccountRequestDto request)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
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

    public async Task<Result<AccountResponseDto>> ToggleStatusAsync(int id)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        if (account == null)
            return Result<AccountResponseDto>.Failure(AccountError.AccountNotFound, ErrorType.NotFound);

        account.IsActive = !account.IsActive;
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AccountResponseDto>(account);
        return Result<AccountResponseDto>.Success(response);
    }

}