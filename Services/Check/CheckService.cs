using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Checks;
using BackEnd.DTOs.Responses.Checks;
using BackEnd.Utils;
using BackEnd.Services.Interfaces;
using BackEnd.Infrastructure.Context;

namespace BackEnd.Services;

public class CheckService : ICheckService
{
    private readonly AppDbContext _context; // Cambia AppDbContext por el nombre de tu DbContext
    private readonly IMapper _mapper;

    public CheckService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CheckResponseDto>>> GetAllAsync()
    {
        var checks = await _context.Checks.ToListAsync();
        var response = _mapper.Map<IEnumerable<CheckResponseDto>>(checks);
        return Result<IEnumerable<CheckResponseDto>>.Success(response);
    }

    public async Task<Result<CheckResponseDto>> GetByIdAsync(int id)
    {
        var check = await _context.Checks.FindAsync(id);
        if (check == null)
            return Result<CheckResponseDto>.Failure("El cheque no existe.", ErrorType.NotFound);

        var response = _mapper.Map<CheckResponseDto>(check);
        return Result<CheckResponseDto>.Success(response);
    }

    public async Task<Result<CheckResponseDto>> CreateAsync(CreateCheckRequestDto request)
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

        // 5. Devolver Response DTO
        var response = _mapper.Map<CheckResponseDto>(newCheck);
        return Result<CheckResponseDto>.Success(response);
    }

    public async Task<Result<CheckResponseDto>> UpdateStatusAsync(int id, UpdateCheckStatusRequestDto request)
    {
        var check = await _context.Checks.FindAsync(id);
        if (check == null)
            return Result<CheckResponseDto>.Failure("El cheque no existe.", ErrorType.NotFound);

        // Actualizamos solo lo permitido por el DTO estricto
        check.Status = request.Status;

        if (request.Status == CheckStatusEnum.Cashed && request.PaymentDate.HasValue)
        {
            check.PaymentDate = request.PaymentDate;
        }

        await _context.SaveChangesAsync();

        var response = _mapper.Map<CheckResponseDto>(check);
        return Result<CheckResponseDto>.Success(response);
    }
}