using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Bank;
using BackEnd.DTOs.Responses.Bank;
using BackEnd.DTOs.Requests.Bank.BankMovement;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Bank.BankMovement;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class BankMovementService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBankMovementsWrapperDto>> GetAllAsync()
    {
        var movements = await _context.BankMovements
            .AsNoTracking()
            .OrderByDescending(bm => bm.Date)
            .ProjectTo<BankMovementResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListBankMovementsWrapperDto>.Success(new ListBankMovementsWrapperDto { BankMovements = movements });
    }

    public async Task<Result<ListBankMovementsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.BankMovements.AsNoTracking();

        var totalElements = await query.CountAsync();

        var movements = await query
            .OrderByDescending(bm => bm.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BankMovementResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBankMovementsWrapperDto>.Success(new ListBankMovementsWrapperDto { BankMovements = movements, Pagination = _pagination });
    }

    public async Task<Result<BankMovementWrapperDto>> GetByIdAsync(int id)
    {
        var movement = await _context.BankMovements
            .AsNoTracking()
            .Where(bm => bm.Id == id)
            .ProjectTo<BankMovementResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (movement == null)
            return Result<BankMovementWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<BankMovementWrapperDto>.Success(new BankMovementWrapperDto { BankMovement = movement });
    }

    public async Task<Result<BankMovementWrapperDto>> CreateAsync(BankMovementRequestDto request)
{
    var account = await _context.Accounts
    .Include(a => a.Bank)
    .FirstOrDefaultAsync(a => a.Id == request.AccountId);
    if (account == null)
        return Result<BankMovementWrapperDto>.Failure(AccountError.AccountNotFound, ErrorType.NotFound);

        // 2. Mapeamos el DTO a Entidad
        var newMovement = _mapper.Map<BankMovement>(request);

        // Nos aseguramos de que la fecha sea la de hoy si no enviaron una
        if (newMovement.Date == default)
            newMovement.Date = DateTime.Now;

        // 3. REGLA DE NEGOCIO CRÍTICA: Actualizar saldos de la cuenta
        if (newMovement.MovementType == BankMovementTypeEnum.Credit)
        {
            account.CurrentBalance += newMovement.Amount;
            account.AvailableBalance += newMovement.Amount;
        }
        else if (newMovement.MovementType == BankMovementTypeEnum.Debit)
        {
            // Validar que haya saldo suficiente antes de restar
            if (account.AvailableBalance < newMovement.Amount)
                return Result<BankMovementWrapperDto>.Failure(AccountError.NotEnoughFunds, ErrorType.Validation);

        account.CurrentBalance -= newMovement.Amount;
        account.AvailableBalance -= newMovement.Amount;
    }

    // 3. ¡LA NUEVA MAGIA DEL CHEQUE!
    if (request.CheckDetails != null)
    {
        var newCheck = _mapper.Map<Check>(request.CheckDetails);
        newCheck.AccountId = newMovement.AccountId;
        newCheck.Amount = newMovement.Amount;
        newCheck.Status = CheckStatusEnum.Pending;

        if (newMovement.MovementType == BankMovementTypeEnum.Debit)
        {
            newCheck.IssuingBank = account.Bank?.Name ?? "";
        }
        else
        {
            newCheck.Receiver = account.Name;
        }

        newMovement.Check = newCheck;
    }

    _context.BankMovements.Add(newMovement);   
    await _context.SaveChangesAsync();

        return await GetByIdAsync(newMovement.Id);
    }

    public async Task<Result<BankMovementDto>> CreateMovementAsync(CreateBankMovementDto request)
    {
        var accountValidation = await ValidateAccountAsync(request.AccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<BankMovementDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        var account = await _context.Accounts.FindAsync(request.AccountId);
        if (account == null)
            return Result<BankMovementDto>.Failure(AccountError.AccountNotFound, ErrorType.NotFound);

        var movement = new Models.BankMovement
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            Date = request.Date == default ? DateTime.UtcNow : request.Date,
            ReferenceNumber = request.ReferenceNumber,
            MovementType = request.MovementType
        };

        if (movement.MovementType == BankMovementTypeEnum.Credit)
        {
            account.CurrentBalance += movement.Amount;
            account.AvailableBalance += movement.Amount;
        }
        else
        {
            account.CurrentBalance -= movement.Amount;
            account.AvailableBalance -= movement.Amount;
        }

        _context.BankMovements.Add(movement);
        await _context.SaveChangesAsync();

        return Result<BankMovementDto>.Success(new BankMovementDto
        {
            Id = movement.Id,
            AccountId = movement.AccountId,
            Amount = movement.Amount,
            Date = movement.Date,
            ReferenceNumber = movement.ReferenceNumber,
            MovementType = movement.MovementType
        });
    }

    public async Task<Result<bool>> ValidateAccountAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
            return Result<bool>.Failure(AccountError.InvalidAmount, ErrorType.Validation);

        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return Result<bool>.Failure(AccountError.AccountNotFound, ErrorType.NotFound);

        if (account.AvailableBalance < amount)
            return Result<bool>.Failure(AccountError.NotEnoughFunds, ErrorType.Validation);

        return Result<bool>.Success(true);
    }
}