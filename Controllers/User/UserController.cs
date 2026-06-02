using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackEnd.DTOs.Responses.User;
using BackEnd.DTOs.Requests.User;
using System.Security.Claims;
using BackEnd.Services;
using BackEnd.Extensions;
using BackEnd.Utils;
using BackEnd.DTOs.Requests.Pagination;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.User;

[Route("api/users")]
[ApiController]
[Authorize] // Use this if you want this controller to be accessible only by admins.
// Use this if you want this controller to be accessible only by admins. 
// [Authorize]
public class UserController(UserService usuarioService) : ControllerBase
{
    private readonly UserService _usuarioService = usuarioService;

    [HttpGet()]
    [HasPermission("users.view")]
    public async Task<ActionResult<ListUsersWrapperDto>> GetListUsers([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _usuarioService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        // Handle other error types as needed
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("users.view")]
    public async Task<ActionResult<UserWrapperDto>> GetUserById(string id)
    {
        var result = await _usuarioService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("users.update")]
    public async Task<ActionResult<UserWrapperDto>> Update(int id, UpdateUserRequestDto request)
    {
        var result = await _usuarioService.UpdateAsync(id, request);
        
        if (result.IsSuccess)
            return Ok(result.Value);
            
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
            
        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);
            
        return StatusCode(500);
    }

    [HttpPatch("{id}/status")]
    [HasPermission("users.update")]
    public async Task<ActionResult> ToggleStatus(int id)
    {
        var result = await _usuarioService.ToggleStatusAsync(id);
        
        if (result.IsSuccess)
            return NoContent();
            
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
            
        return StatusCode(500);
    }

    [HttpPut("{id}/role")]
    [HasPermission("users.update")]
    public async Task<ActionResult<UserWrapperDto>> UpdateRole(int id, [FromBody] UpdateUserRoleRequestDto request)
    {
        var result = await _usuarioService.UpdateRoleAsync(id, request.RoleId);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
