using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
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
    var account = await _context.Accounts.FindAsync(request.AccountId);
    if (account == null)
        return Result<BankMovementWrapperDto>.Failure("Cuenta no encontrada", ErrorType.NotFound);

    // 1. Mapeamos el movimiento
    var newMovement = _mapper.Map<BankMovement>(request);
    if (newMovement.Date == default) newMovement.Date = DateTime.Now;

    // 2. Lógica de saldos (como ya la tenías)
    if (newMovement.MovementType == BankMovementTypeEnum.Credit) 
    {
        account.CurrentBalance += newMovement.Amount;
        account.AvailableBalance += newMovement.Amount;
    }
    else if (newMovement.MovementType == BankMovementTypeEnum.Debit)
    {
        if (account.AvailableBalance < newMovement.Amount)
            return Result<BankMovementWrapperDto>.Failure("Saldo insuficiente", ErrorType.Validation);

        account.CurrentBalance -= newMovement.Amount;
        account.AvailableBalance -= newMovement.Amount;
    }

    // 3. ¡LA NUEVA MAGIA DEL CHEQUE!
    if (request.CheckDetails != null)
    {
        var newCheck = _mapper.Map<Check>(request.CheckDetails);
        newCheck.Status = CheckStatusEnum.Pending; // Nace pendiente de conciliación
        
        // Entity Framework es inteligente: al asignarlo a la propiedad de navegación,
        // automáticamente le pondrá el BankMovementId correcto cuando guarde.
        newMovement.Check = newCheck; 
    }

    _context.BankMovements.Add(newMovement);
    await _context.SaveChangesAsync();

    return await GetByIdAsync(newMovement.Id);
}
}