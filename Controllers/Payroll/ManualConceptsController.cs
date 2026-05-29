using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Payroll;

[Route("api/manual-concepts")]
[ApiController]
[Authorize]
public class ManualConceptsController(PayrollProcessingService payrollProcessingService) : ControllerBase
{
    private readonly PayrollProcessingService _payrollProcessingService = payrollProcessingService;

    [HttpPost]
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

    [HttpGet("pending")]
    public async Task<ActionResult<List<ManualConceptIncidentResponseDto>>> GetPending()
    {
        var result = await _payrollProcessingService.GetPendingManualConceptIncidentsAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }
}