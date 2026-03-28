using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackEnd.DTOs.Responses.User;
using System.Security.Claims;
using BackEnd.Services;
using BackEnd.Extensions;

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
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from token
        var result = await _usuarioService.GetProfileAsync(userId);
        
        if (!result.IsSuccess)
            return this.HandleNotFoundProblem(result);
            
        return Ok(result.Value);
    }
}