using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Bank.BankMovement; 
using BackEnd.DTOs.Responses.Bank.BankMovement;
using BackEnd.Utils;
using BackEnd.Services.Interfaces;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class BankMovementService : IBankMovementService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public BankMovementService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<BankMovementResponseDto>>> GetAllAsync()
    {
        var movements = await _context.BankMovements
            .Include(bm => bm.Account) // Traemos los datos de la cuenta asociada
            .AsNoTracking()
            .OrderByDescending(bm => bm.Date) // Lo más normal es ver los últimos movimientos primero
            .ToListAsync();

        var response = _mapper.Map<IEnumerable<BankMovementResponseDto>>(movements);
        return Result<IEnumerable<BankMovementResponseDto>>.Success(response);
    }

    public async Task<Result<BankMovementResponseDto>> GetByIdAsync(int id)
    {
        var movement = await _context.BankMovements
            .Include(bm => bm.Account)
            .AsNoTracking()
            .FirstOrDefaultAsync(bm => bm.Id == id);

        if (movement == null)
            return Result<BankMovementResponseDto>.Failure("El movimiento no existe.", ErrorType.NotFound);

        var response = _mapper.Map<BankMovementResponseDto>(movement);
        return Result<BankMovementResponseDto>.Success(response);
    }

    public async Task<Result<BankMovementResponseDto>> CreateAsync(BankMovementRequestDto request)
    {
        // 1. Verificamos que la cuenta exista
        var account = await _context.Accounts.FindAsync(request.AccountId);
        if (account == null)
            return Result<BankMovementResponseDto>.Failure("La cuenta bancaria seleccionada no existe.", ErrorType.NotFound);

        // 2. Mapeamos el DTO a Entidad
        var newMovement = _mapper.Map<BankMovement>(request);
        
        // Nos aseguramos de que la fecha sea la de hoy si no enviaron una
        if (newMovement.Date == default) 
            newMovement.Date = DateTime.Now;

        // 3. REGLA DE NEGOCIO CRÍTICA: Actualizar saldos de la cuenta
        // ¡OJO! Cambia "Income" y "Expense" por los nombres exactos que le diste a tu enum
        if (newMovement.MovementType == BankMovementTypeEnum.Credit) 
        {
            account.CurrentBalance += newMovement.Amount;
            account.AvailableBalance += newMovement.Amount;
        }
        else if (newMovement.MovementType == BankMovementTypeEnum.Debit)
        {
            // Opcional: Validar que haya saldo suficiente antes de restar
            if (account.AvailableBalance < newMovement.Amount)
                return Result<BankMovementResponseDto>.Failure("Saldo insuficiente para realizar este movimiento.", ErrorType.Validation);

            account.CurrentBalance -= newMovement.Amount;
            account.AvailableBalance -= newMovement.Amount;
        }

        // 4. Guardamos todo. Como modificamos 'account' y agregamos 'newMovement', 
        // EF Core hará todo en una sola transacción segura cuando llamemos a SaveChangesAsync
        _context.BankMovements.Add(newMovement);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<BankMovementResponseDto>(newMovement);
        return Result<BankMovementResponseDto>.Success(response);
    }
}