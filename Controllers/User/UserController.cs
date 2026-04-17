using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackEnd.DTOs.Responses.User;
using System.Security.Claims;
using BackEnd.Services;
using BackEnd.Extensions;
using BackEnd.Utils;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Controllers.User;

[Route("api/users")]
[ApiController]
[Authorize(Roles = "Admin")] // Use this if you want this controller to be accessible only by admins.
// Use this if you want this controller to be accessible only by admins. 
// [Authorize(Roles = "Admin")]
public class UserController(UserService usuarioService) : ControllerBase
{
    private readonly UserService _usuarioService = usuarioService;

    [HttpGet()]
    public async Task<ActionResult<ListUsersWrapperDto>> GetListUsers([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _usuarioService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        // Handle other error types as needed
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserWrapperDto>> GetUserById(string id)
    {
        var result = await _usuarioService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

}