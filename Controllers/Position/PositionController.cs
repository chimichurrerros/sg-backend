using BackEnd.DTOs.Requests.Position;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Position;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using BackEnd.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Position;

[Route("api/positions")]
[ApiController]
[Authorize]
public class PositionController(PositionService positionService) : ControllerBase
{
    private readonly PositionService _positionService = positionService;

    [HttpGet]
    [HasPermission("positions.view")]
    public async Task<ActionResult<ListPositionsWrapperDto>> GetAll([FromQuery] OrganizationQueryDto query, [FromQuery] int? departmentId = null)
    {
        var result = await _positionService.GetAllAsync(query, departmentId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("positions.create")]
    public async Task<ActionResult<PositionWrapperDto>> Create(PositionRequestDto request)
    {
        var result = await _positionService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/positions/{result.Value!.Position.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("positions.update")]
    public async Task<ActionResult<PositionWrapperDto>> Update(int id, PositionRequestDto request)
    {
        var result = await _positionService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("positions.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _positionService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
