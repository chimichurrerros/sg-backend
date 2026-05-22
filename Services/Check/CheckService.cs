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

public async Task<Result<CheckWrapperDto>> ConciliateAsync(int id)
{
    var check = await _context.Checks.FirstOrDefaultAsync(c => c.Id == id);

    if (check == null)
        return Result<CheckWrapperDto>.Failure("El cheque no existe.", ErrorType.NotFound);

    if (check.Status == CheckStatusEnum.Cashed)
        return Result<CheckWrapperDto>.Failure("El cheque ya está conciliado.", ErrorType.Validation);

    check.Status = CheckStatusEnum.Cashed; 
    check.ConciliationDate = DateOnly.FromDateTime(DateTime.Now);

    await _context.SaveChangesAsync();

    // --- LA SOLUCIÓN AL AUTOMAPPER EXCEPTION ESTÁ AQUÍ ---
    
    // 1. Mapeamos la entidad Check al DTO que sí está configurado en tu perfil
    var checkDto = _mapper.Map<CheckResponseDto>(check);
    
    // 2. Armamos el Wrapper a mano
    var response = new CheckWrapperDto { Check = checkDto };

    // 3. Devolvemos el resultado
    return Result<CheckWrapperDto>.Success(response);
    } 

}
