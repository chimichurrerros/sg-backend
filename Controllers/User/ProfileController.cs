using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Services;
using BackEnd.Models.Responses;
using BackEnd.Models.Responses.User;

namespace BackEnd.Controllers.User;

[Route("api/profile")]
[ApiController]
[Authorize]
// Use this if you want this controller to be accessible only by admins. 
// [Authorize(Roles = "Admin")]
public class ProfileController(UsuarioService usuarioService) : ControllerBase
{
    private readonly UsuarioService _usuarioService = usuarioService;

    [HttpGet()]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto?>>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from token
        var profile = await _usuarioService.GetProfileAsync(userId);
        return profile.Success ? Ok(profile) : NotFound(profile);
    }
}