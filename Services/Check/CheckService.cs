using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Checks;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Checks;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class CheckService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListChecksWrapperDto>> GetAllAsync()
    {
        var checks = await _context.Checks
            .AsNoTracking()
            .ProjectTo<CheckResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListChecksWrapperDto>.Success(new ListChecksWrapperDto { Checks = checks });
    }

    public async Task<Result<ListChecksWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Checks.AsNoTracking();

        var totalElements = await query.CountAsync();

        var checks = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<CheckResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListChecksWrapperDto>.Success(new ListChecksWrapperDto { Checks = checks, Pagination = _pagination });
    }

    public async Task<Result<CheckWrapperDto>> GetByIdAsync(int id)
    {
        var check = await _context.Checks
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<CheckResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (check == null)
            return Result<CheckWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<CheckWrapperDto>.Success(new CheckWrapperDto { Check = check });
    }

    public async Task<Result<CheckWrapperDto>> CreateAsync(CreateCheckRequestDto request)
    {
        // 1. Mapear DTO a Entidad
        var newCheck = _mapper.Map<Check>(request);

        // 2. Regla: Un cheque nuevo siempre nace Pendiente
        newCheck.Status = CheckStatusEnum.Pending;

        // 3. Regla: Cálculo de fechas según el tipo de cheque
        if (newCheck.Type == CheckTypeEnum.Day)
        {
            // Cheque al día: Disponibilidad = Emisión, Vence en 30 días
            newCheck.AvailabilityDate = DateOnly.FromDateTime(request.EmisionDate);
            newCheck.MaturityDate = newCheck.AvailabilityDate.AddDays(30);
        }
        else if (newCheck.Type == CheckTypeEnum.Deferred)
        {
            // Diferido: Vence en 6 meses desde la fecha de disponibilidad enviada
            // Asumimos que el frontend mandó AvailabilityDate, si no, toma la de hoy
            newCheck.AvailabilityDate = request.AvailabilityDate ?? DateOnly.FromDateTime(DateTime.Today);
            newCheck.MaturityDate = newCheck.AvailabilityDate.AddMonths(6);
        }

        // 4. Guardar en Base de Datos
        _context.Checks.Add(newCheck);
        await _context.SaveChangesAsync();

        // 5. Devolver Response DTO recargándolo
        return await GetByIdAsync(newCheck.Id);
    }

    // Fíjate que ahora recibe el DTO
    public async Task<Result<CheckWrapperDto>> UpdateStatusAsync(int id, UpdateCheckStatusRequestDto request)
    {
        var check = await _context.Checks.FirstOrDefaultAsync(c => c.Id == id);

        if (check == null)
            return Result<CheckWrapperDto>.Failure("El cheque no existe.", ErrorType.NotFound);

        // Leemos el nuevo estado desde la propiedad de tu DTO (asumo que se llama Status)
        if (check.Status == request.Status)
            return Result<CheckWrapperDto>.Failure("El cheque ya se encuentra en el estado solicitado.", ErrorType.Validation);

        if (request.Status == CheckStatusEnum.Cashed)
        {
            var account = await _context.Accounts.FindAsync(check.AccountId);
            if (account == null)
                return Result<CheckWrapperDto>.Failure("La cuenta bancaria asociada a este cheque no existe.", ErrorType.NotFound);

            check.PaymentDate = DateOnly.FromDateTime(DateTime.Now);

            var movement = new BankMovement
            {
                AccountId = check.AccountId,
                MovementType = BankMovementTypeEnum.Debit,
                Date = DateTime.Now,
                Amount = check.Amount,
                ReferenceNumber = $"CHQ-{check.Number} (COBRO)"
            };
            
            _context.BankMovements.Add(movement);

            account.CurrentBalance -= check.Amount;
            account.AvailableBalance -= check.Amount;
        }
        else if (request.Status == CheckStatusEnum.Voided) 
        {
            check.PaymentDate = null;
        }

        check.Status = request.Status;

        await _context.SaveChangesAsync();

        var response = _mapper.Map<CheckWrapperDto>(check);
        return Result<CheckWrapperDto>.Success(response);
    }
}
