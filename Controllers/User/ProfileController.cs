using Microsoft.AspNetCore.Mvc;
using BackEnd.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackEnd.Models.Constants;
using BackEnd.Services;

namespace BackEnd.Controllers.User1;

[Route("api/profile")]
[ApiController]
[Authorize]

// ESTO ES SOLO PARA PRUEBAS DEBERIAMOS DE CREAR UN SERVICES PARA SERPARAR 
// LA FUNCIONALIDAD
// Los controladores solo deberian de servir para enviar y recibir datos, 
// la logica de negocio deberia de estar en los services

public class ProfileController(IUsuarioService usuarioService) : ControllerBase
{
    private readonly IUsuarioService _usuarioService = usuarioService;

    [HttpGet()]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from JWT claims
        var profile = await _usuarioService.GetProfileAsync(userId);

        if (profile == null)
        {
            string errorMessage = ApplicationError.NotFoundError.UserNotFound;
            return NotFound(new ApiResponseDto
            {
                Success = false,
                Message = errorMessage,
                Errors = new { User = new[] { errorMessage } }
            });
        }

        return Ok(new ApiResponseDto
        {
            Success = true,
            Message = ApplicationMessages.Success.UserProfileRetrieved,
            Data = profile
        });
    }
}