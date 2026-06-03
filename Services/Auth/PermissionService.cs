using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Permission;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Permission;
using BackEnd.Models;

namespace BackEnd.Services;

public class PermissionService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListPermissionsWrapperDto>> GetAllAsync()
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Include(p => p.Role)
            .ProjectTo<PermissionResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListPermissionsWrapperDto>.Success(new ListPermissionsWrapperDto { Permissions = permissions });
    }

    public async Task<Result<ListPermissionsWrapperDto>> GetListAsync(PermissionQueryDto queryDto)
    {
        var query = _context.Permissions.AsNoTracking().Include(p => p.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(queryDto.Name.ToLower()));
        }

        if (queryDto.RoleId.HasValue)
        {
            query = query.Where(p => p.RoleId == queryDto.RoleId.Value);
        }

        var totalElements = await query.CountAsync();

        var permissions = await query
            .OrderBy(p => p.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<PermissionResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListPermissionsWrapperDto>.Success(new ListPermissionsWrapperDto { Permissions = permissions, Pagination = pagination });
    }

    public async Task<Result<PermissionWrapperDto>> GetByIdAsync(Guid id)
    {
        var permission = await _context.Permissions
            .AsNoTracking()
            .Include(p => p.Role)
            .Where(p => p.Id == id)
            .ProjectTo<PermissionResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (permission == null)
            return Result<PermissionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<PermissionWrapperDto>.Success(new PermissionWrapperDto { Permission = permission });
    }

    public async Task<Result<PermissionWrapperDto>> CreateAsync(PermissionRequestDto request)
    {
        // Check if role exists
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
        if (!roleExists)
        {
            return Result<PermissionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);
        }

        // Check if permission already exists for this role to prevent duplicates
        var alreadyExists = await _context.Permissions.AnyAsync(p => p.Name.ToLower() == request.Name.ToLower() && p.RoleId == request.RoleId);
        if (alreadyExists)
        {
            return Result<PermissionWrapperDto>.Failure(ApplicationError.ValidationFailed, ErrorType.Validation);
        }

        var permission = _mapper.Map<Permission>(request);

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(permission.Id);
    }

    public async Task<Result<PermissionWrapperDto>> UpdateAsync(Guid id, PermissionRequestDto request)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return Result<PermissionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        // Check if role exists
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
        if (!roleExists)
        {
            return Result<PermissionWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);
        }

        // Check if permission name and role combination is already taken by another permission
        var alreadyExists = await _context.Permissions.AnyAsync(p => p.Name.ToLower() == request.Name.ToLower() && p.RoleId == request.RoleId && p.Id != id);
        if (alreadyExists)
        {
            return Result<PermissionWrapperDto>.Failure(ApplicationError.ValidationFailed, ErrorType.Validation);
        }

        _mapper.Map(request, permission);

        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(permission.Id);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
