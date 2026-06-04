using BackEnd.DTOs.Requests.Role;
using BackEnd.DTOs.Responses.Role;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Infrastructure.Authorization;

namespace BackEnd.Controllers.Auth;

[Route("api/roles")]
[ApiController]
[Authorize]
public class RolesController(RoleService roleService) : ControllerBase
{
    private readonly RoleService _roleService = roleService;

    [HttpGet]
    [HasPermission("roles.view")]
    public async Task<ActionResult<ListRolesWrapperDto>> GetAllRoles()
    {
        var result = await _roleService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<RoleWrapperDto>> GetRoleById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("roles.create")]
    public async Task<ActionResult<RoleWrapperDto>> Create([FromBody] RoleRequestDto request)
    {
        var result = await _roleService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/roles/{result.Value!.Role.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("roles.update")]
    public async Task<ActionResult<RoleWrapperDto>> Update(int id, [FromBody] RoleRequestDto request)
    {
        var result = await _roleService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("roles.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPost("{id}/permissions")]
    [HasPermission("roles.update")]
    public async Task<ActionResult<RoleWrapperDto>> SyncPermissions(int id, [FromBody] SyncRolePermissionsRequestDto request)
    {
        var result = await _roleService.SyncPermissionsAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
