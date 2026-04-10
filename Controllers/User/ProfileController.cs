using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackEnd.DTOs.Responses.User;
using System.Security.Claims;
using BackEnd.Services;
using BackEnd.Extensions;
using BackEnd.Utils;

namespace BackEnd.Controllers.User;

[Route("api/profile")]
[ApiController]
[Authorize]
// Use this if you want this controller to be accessible only by admins. 
// [Authorize(Roles = "Admin")]
public class ProfileController(UserService usuarioService) : ControllerBase
{
    private readonly UserService _usuarioService = usuarioService;

    [HttpGet()]
    public async Task<ActionResult<UserWrapperDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from token
        var result = await _usuarioService.GetByIdAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        // This is only a example xd
        return StatusCode(500);
    }

}