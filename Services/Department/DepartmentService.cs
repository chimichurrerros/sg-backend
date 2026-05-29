using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Department;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Department;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class DepartmentService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListDepartmentsWrapperDto>> GetAllAsync(OrganizationQueryDto query)
    {
        var departmentsQuery = _context.Departments
            .AsNoTracking()
            .Where(d => string.IsNullOrWhiteSpace(query.Search) || d.Name.ToLower().Contains(query.Search.ToLower()));

        departmentsQuery = ApplySort(departmentsQuery, query.SortBy, query.SortOrder);

        var totalElements = await departmentsQuery.CountAsync();

        var departments = await departmentsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<DepartmentResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pagination = new Pagination(query.Page, query.PageSize, totalElements);

        return Result<ListDepartmentsWrapperDto>.Success(new ListDepartmentsWrapperDto { Departments = departments, Pagination = pagination });
    }

    public async Task<Result<DepartmentWrapperDto>> GetByIdAsync(int id)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .ProjectTo<DepartmentResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (department == null)
            return Result<DepartmentWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<DepartmentWrapperDto>.Success(new DepartmentWrapperDto { Department = department });
    }

    public async Task<Result<DepartmentWrapperDto>> CreateAsync(DepartmentRequestDto request)
    {
        var department = _mapper.Map<Department>(request);

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(department.Id);
    }

    public async Task<Result<DepartmentWrapperDto>> UpdateAsync(int id, DepartmentRequestDto request)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
            return Result<DepartmentWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, department);
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(department.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private static IQueryable<Department> ApplySort(IQueryable<Department> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? "id").ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            _ => desc ? query.OrderByDescending(d => d.Id) : query.OrderBy(d => d.Id)
        };
    }
}