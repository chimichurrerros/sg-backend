using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Position;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Position;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PositionService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListPositionsWrapperDto>> GetAllAsync(OrganizationQueryDto query, int? departmentId = null)
    {
        var positionsQuery = _context.Positions
            .AsNoTracking()
            .Where(p => (string.IsNullOrWhiteSpace(query.Search) || p.Name.ToLower().Contains(query.Search.ToLower())));

        if (departmentId.HasValue)
        {
            positionsQuery = departmentId.Value == 0
                ? positionsQuery.Where(p => p.DepartmentId == null)
                : positionsQuery.Where(p => p.DepartmentId == departmentId.Value);
        }

        positionsQuery = ApplySort(positionsQuery, query.SortBy, query.SortOrder);

        var totalElements = await positionsQuery.CountAsync();

        var positions = await positionsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(p => p.Department)
            .ProjectTo<PositionResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pagination = new Pagination(query.Page, query.PageSize, totalElements);

        return Result<ListPositionsWrapperDto>.Success(new ListPositionsWrapperDto { Positions = positions, Pagination = pagination });
    }

    public async Task<Result<PositionWrapperDto>> GetByIdAsync(int id)
    {
        var position = await _context.Positions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectTo<PositionResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (position == null)
            return Result<PositionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<PositionWrapperDto>.Success(new PositionWrapperDto { Position = position });
    }

    public async Task<Result<PositionWrapperDto>> CreateAsync(PositionRequestDto request)
    {
        var position = _mapper.Map<Position>(request);

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(position.Id);
    }

    public async Task<Result<PositionWrapperDto>> UpdateAsync(int id, PositionRequestDto request)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position == null)
            return Result<PositionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, position);
        _context.Positions.Update(position);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(position.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private static IQueryable<Position> ApplySort(IQueryable<Position> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? "id").ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "defaultbasicsalary" => desc ? query.OrderByDescending(p => p.DefaultBasicSalary) : query.OrderBy(p => p.DefaultBasicSalary),
            _ => desc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };
    }
}