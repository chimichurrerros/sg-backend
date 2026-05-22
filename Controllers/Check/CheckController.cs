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

    

   [HttpPatch("{id}/conciliate")] 
public async Task<IActionResult> Conciliate(int id)
{
    // Ahora llamamos al nuevo método que creamos en el CheckService
    var response = await _checkService.ConciliateAsync(id);

    if (!response.IsSuccess)
        return BadRequest(new { Error = response.Errors, Type = response.ErrorType });

    return Ok(response.Value);
}
}