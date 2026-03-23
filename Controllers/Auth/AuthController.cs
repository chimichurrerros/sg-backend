using BackEnd.Models.Requests.Auth;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var response = await _authService.Logout();
        return response.Success ? Ok(response) : BadRequest(response);
    }
}