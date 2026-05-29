using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Payroll;

[Route("api/payroll-processes")]
[ApiController]
[Authorize]
public class PayrollProcessesController(PayrollProcessingService payrollProcessingService) : ControllerBase
{
    private readonly PayrollProcessingService _payrollProcessingService = payrollProcessingService;

    [HttpPost("{processId}/manual-details")]
    public async Task<ActionResult<PayrollManualDetailResponseDto>> UpsertManualDetail(int processId, PayrollManualInputDto request)
    {
        var result = await _payrollProcessingService.UpsertManualDetailAsync(processId, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }

    [HttpGet("{processId}/manual-details")]
    public async Task<ActionResult<List<PayrollManualDetailResponseDto>>> GetManualDetails(int processId)
    {
        var result = await _payrollProcessingService.GetManualDetailsAsync(processId);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("manual-details/{id}")]
    public async Task<ActionResult> DeleteManualDetail(int id)
    {
        var result = await _payrollProcessingService.DeleteManualDetailAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }

    [HttpPost("{id}/calculate")]
    public async Task<ActionResult<PayrollProcessCalculationResponseDto>> Calculate(int id)
    {
        var result = await _payrollProcessingService.CalculateAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }
}