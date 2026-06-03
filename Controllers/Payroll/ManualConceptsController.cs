using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Payroll;

[Route("api/manual-concepts")]
[ApiController]
[Authorize]
public class ManualConceptsController(PayrollProcessingService payrollProcessingService) : ControllerBase
{
    private readonly PayrollProcessingService _payrollProcessingService = payrollProcessingService;

    [HttpPost]
    [HasPermission("manualConcepts.create")]
    public async Task<ActionResult<ManualConceptIncidentResponseDto>> Create(ManualConceptIncidentCreateDto request)
    {
        var result = await _payrollProcessingService.CreateManualConceptIncidentAsync(request);

        if (result.IsSuccess)
            return Created($"/api/manual-concepts/{result.Value!.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id:int}")]
    [HasPermission("manualConcepts.update")]
    public async Task<ActionResult<ManualConceptIncidentResponseDto>> Update(int id, ManualConceptIncidentCreateDto request)
    {
        var result = await _payrollProcessingService.UpdateManualConceptIncidentAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return this.HandleConflictProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id:int}")]
    [HasPermission("manualConcepts.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _payrollProcessingService.DeleteManualConceptIncidentAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return this.HandleConflictProblem(result);

        return StatusCode(500);
    }

    [HttpGet("pending")]
    [HasPermission("manualConcepts.view")]
    public async Task<ActionResult<List<ManualConceptIncidentResponseDto>>> GetPending()
    {
        var result = await _payrollProcessingService.GetPendingManualConceptIncidentsAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }
}
