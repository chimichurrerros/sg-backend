using BackEnd.DTOs.Requests.Permission;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Permission;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Infrastructure.Authorization;

namespace BackEnd.Controllers.Auth;

[Route("api/permissions")]
[ApiController]
[Authorize]
public class PermissionController(PermissionService permissionService) : ControllerBase
{
    private readonly PermissionService _permissionService = permissionService;

    [HttpGet]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<ListPermissionsWrapperDto>> GetListPermissions([FromQuery] PermissionQueryDto query)
    {
        var result = await _permissionService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<ListPermissionsWrapperDto>> GetAllPermissions()
    {
        var result = await _permissionService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<PermissionWrapperDto>> GetPermissionById(Guid id)
    {
        var result = await _permissionService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("permissions.create")]
    public async Task<ActionResult<PermissionWrapperDto>> Create([FromBody] PermissionRequestDto request)
    {
        var result = await _permissionService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/permissions/{result.Value!.Permission.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("permissions.update")]
    public async Task<ActionResult<PermissionWrapperDto>> Update(Guid id, [FromBody] PermissionRequestDto request)
    {
        var result = await _permissionService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("permissions.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _permissionService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
