using BackEnd.DTOs.Requests.Checks;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Checks;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Check;

[Route("api/checks")]
[ApiController]
[Authorize]
[AllowAnonymous]

public class ChecksController(CheckService checkService) : ControllerBase
{
    private readonly CheckService _checkService = checkService;

    [HttpGet]
    public async Task<ActionResult<ListChecksWrapperDto>> GetListChecks([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _checkService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListChecksWrapperDto>> GetAllChecks()
    {
        var result = await _checkService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CheckWrapperDto>> GetById(int id)
    {
        var result = await _checkService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<CheckWrapperDto>> Create(CreateCheckRequestDto request)
    {
        var result = await _checkService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/checks/{result.Value!.Check.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    // Usamos PATCH porque es una actualización parcial (solo estado y fecha)
  [HttpPatch("{id}/status")]
    public async Task<ActionResult<CheckWrapperDto>> UpdateStatus(int id, [FromBody] UpdateCheckStatusRequestDto request)
    {
        var result = await _checkService.UpdateStatusAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result); // Tu manejador personalizado

        if (result.ErrorType == ErrorType.Validation)
            return BadRequest(result); // ¡Añadimos esto para manejar reglas de negocio!

        return StatusCode(500, "Ocurrió un error interno en el servidor.");
    }
}