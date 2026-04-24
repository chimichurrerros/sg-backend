using BackEnd.DTOs.Requests.Auth;
using BackEnd.DTOs.Responses.User;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Extensions;

namespace BackEnd.Controllers.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (result.IsSuccess)
            return Ok();

        return this.HandleValidationProblem(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserWrapperDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (result.IsSuccess)
            return Ok(result.Value);

        return this.HandleValidationProblem(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.Logout();
        
        if (result.IsSuccess)
            return Ok();

        return this.HandleValidationProblem(result);
    }
}