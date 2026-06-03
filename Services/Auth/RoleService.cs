using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Role;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Role;
using BackEnd.Models;
using System.Collections.Generic;
using System.Linq;

namespace BackEnd.Services;

public class RoleService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListRolesWrapperDto>> GetAllAsync()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .ProjectTo<RoleResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListRolesWrapperDto>.Success(new ListRolesWrapperDto { Roles = roles });
    }

    public async Task<Result<RoleWrapperDto>> GetByIdAsync(int id)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .Where(r => r.Id == id)
            .ProjectTo<RoleResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (role == null)
            return Result<RoleWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<RoleWrapperDto>.Success(new RoleWrapperDto { Role = role });
    }

    public async Task<Result<RoleWrapperDto>> CreateAsync(RoleRequestDto request)
    {
        var nameLower = request.Name.ToLower();
        var alreadyExists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == nameLower);
        if (alreadyExists)
            return Result<RoleWrapperDto>.Failure(ApplicationError.Conflict, ErrorType.Validation);

        var role = _mapper.Map<Role>(request);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(role.Id);
    }

    public async Task<Result<RoleWrapperDto>> UpdateAsync(int id, RoleRequestDto request)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return Result<RoleWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        var nameLower = request.Name.ToLower();
        var alreadyExists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == nameLower && r.Id != id);
        if (alreadyExists)
            return Result<RoleWrapperDto>.Failure(ApplicationError.Conflict, ErrorType.Validation);

        _mapper.Map(request, role);
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(role.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var role = await _context.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        if (role.Users.Any())
        {
            return Result.Failure("No se puede eliminar el rol porque tiene usuarios asignados.", ErrorType.Validation);
        }

        var permissions = await _context.Permissions.Where(p => p.RoleId == id).ToListAsync();
        _context.Permissions.RemoveRange(permissions);

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<RoleWrapperDto>> SyncPermissionsAsync(int id, SyncRolePermissionsRequestDto request)
    {
        var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return Result<RoleWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Permissions.RemoveRange(role.Permissions);

        foreach (var permissionName in request.Permissions.Distinct())
        {
            var newPermission = new Permission
            {
                Name = permissionName,
                RoleId = id
            };
            _context.Permissions.Add(newPermission);
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }
}
