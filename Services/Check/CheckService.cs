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
            .OrderByDescending(c => c.EmisionDate)
            .ThenByDescending(c => c.Id)
            .ProjectTo<CheckResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListChecksWrapperDto>.Success(new ListChecksWrapperDto { Checks = checks });
    }

    public async Task<Result<ListChecksWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Checks.AsNoTracking();

        var totalElements = await query.CountAsync();

        var checks = await query
            .OrderByDescending(v => v.EmisionDate)
            .ThenByDescending(v => v.Id)
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

public async Task<Result<CheckWrapperDto>> UpdateStatusAsync(int id, UpdateCheckStatusRequestDto request)
{
    var check = await _context.Checks.FirstOrDefaultAsync(c => c.Id == id);

    if (check == null)
        return Result<CheckWrapperDto>.Failure(CheckError.CheckNotFound, ErrorType.NotFound);

    // Evitamos hacer el proceso si ya tiene el estado solicitado
    if (check.Status == request.Status)
        return Result<CheckWrapperDto>.Failure(string.Format(CheckError.CheckAlreadyInStatus, request.Status), ErrorType.Validation);

    // 1. Cambiamos el estado
    check.Status = request.Status;

    // 2. Lógica de Conciliación
    if (request.Status == CheckStatusEnum.Cashed)
    {
        check.ConciliationDate = DateOnly.FromDateTime(DateTime.Now);
    }
    else
    {
        check.ConciliationDate = null; 
    }

    // 3. Guardamos los cambios
    await _context.SaveChangesAsync();

  
    return await GetByIdAsync(check.Id);
}
}