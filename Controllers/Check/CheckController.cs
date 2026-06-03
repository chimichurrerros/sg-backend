using BackEnd.DTOs.Requests.Checks;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Checks;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Check;

[Route("api/checks")]
[ApiController]
[Authorize]
[AllowAnonymous]

public class ChecksController(CheckService checkService) : ControllerBase
{
    private readonly CheckService _checkService = checkService;

    [HttpGet]
    [HasPermission("checks.view")]
    public async Task<ActionResult<ListChecksWrapperDto>> GetListChecks([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _checkService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("checks.view")]
    public async Task<ActionResult<ListChecksWrapperDto>> GetAllChecks()
    {
        var result = await _checkService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("checks.view")]
    public async Task<ActionResult<CheckWrapperDto>> GetById(int id)
    {
        var result = await _checkService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    

[HttpPatch("{id}/status")]
[HasPermission("checks.update")]
public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCheckStatusRequestDto request)
{
    var response = await _checkService.UpdateStatusAsync(id, request);

    if (!response.IsSuccess)
        return BadRequest(new { Error = response.Errors, Type = response.ErrorType });

    return Ok(response.Value);
}
}
