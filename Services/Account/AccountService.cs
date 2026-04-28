using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Accounts;
using BackEnd.DTOs.Responses.Accounts;
using BackEnd.Utils;
using BackEnd.Services.Interfaces;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context; 
    private readonly IMapper _mapper;

    public AccountService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AccountResponseDto>>> GetAllAsync()
    {
        // Usamos Include para traer los datos del banco si es que tiene uno asociado
        var accounts = await _context.Accounts
            .Include(a => a.Bank)
            .ToListAsync();
            
        var response = _mapper.Map<IEnumerable<AccountResponseDto>>(accounts);
        return Result<IEnumerable<AccountResponseDto>>.Success(response);
    }

    public async Task<Result<AccountResponseDto>> GetByIdAsync(int id)
    {
        var account = await _context.Accounts
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
            return Result<AccountResponseDto>.Failure("La cuenta no existe.", ErrorType.NotFound);

        var response = _mapper.Map<AccountResponseDto>(account);
        return Result<AccountResponseDto>.Success(response);
    }

    public async Task<Result<AccountResponseDto>> CreateAsync(CreateAccountRequestDto request)
    {
        var newAccount = _mapper.Map<Account>(request);

        // REGLA DE NEGOCIO: Una cuenta nueva siempre nace con saldo 0
        // (A menos que tu CreateAccountRequestDto explícitamente pida un saldo inicial)
        newAccount.CurrentBalance = 0;
        newAccount.AvailableBalance = 0;

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AccountResponseDto>(newAccount);
        return Result<AccountResponseDto>.Success(response);
    }

    public async Task<Result<AccountResponseDto>> UpdateAsync(int id, UpdateAccountRequestDto request)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
            return Result<AccountResponseDto>.Failure("La cuenta no existe.", ErrorType.NotFound);

        // Mapeamos los datos permitidos. 
        // Importante: Tu UpdateAccountRequestDto NO debería tener CurrentBalance ni AvailableBalance.
        _mapper.Map(request, account);

        await _context.SaveChangesAsync();

        var response = _mapper.Map<AccountResponseDto>(account);
        return Result<AccountResponseDto>.Success(response);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
            return Result<bool>.Failure("La cuenta no existe.", ErrorType.NotFound);

        // REGLA DE NEGOCIO: No se puede borrar una cuenta si ya tiene movimientos
        var hasMovements = await _context.BankMovements.AnyAsync(bm => bm.AccountId == id);
        if (hasMovements)
            return Result<bool>.Failure("No se puede eliminar la cuenta porque ya tiene movimientos registrados.", ErrorType.Validation);

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}