using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Payroll;

[Route("api/payroll-processes")]
[ApiController]
[Authorize]
public class PayrollProcessesController(PayrollProcessingService payrollProcessingService) : ControllerBase
{
    private readonly PayrollProcessingService _payrollProcessingService = payrollProcessingService;

    [HttpPatch("{processId}/status")]
    [HasPermission("payrollProcesses.update")]
    public async Task<ActionResult> UpdateStatus(int processId, UpdatePayrollProcessStatusRequestDto request)
    {
        var result = await _payrollProcessingService.UpdatePayrollProcessStatusAsync(processId, request);

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

    [HttpGet]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<List<PayrollProcessResponseDto>>> GetList()
    {
        var result = await _payrollProcessingService.GetListAsync();
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<PayrollProcessResponseDto>> GetById(int id)
    {
        var result = await _payrollProcessingService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("payrollProcesses.create")]
    public async Task<ActionResult<PayrollProcessResponseDto>> Create(PayrollProcessCreateDto request)
    {
        var result = await _payrollProcessingService.CreatePayrollProcessAsync(request);
        if (result.IsSuccess) return Created($"/api/payroll-processes/{result.Value!.Id}", result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("payrollProcesses.update")]
    public async Task<ActionResult> Update(int id, PayrollProcessUpdateDto request)
    {
        var result = await _payrollProcessingService.UpdatePayrollProcessAsync(id, request);
        if (result.IsSuccess) return NoContent();
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("payrollProcesses.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _payrollProcessingService.DeletePayrollProcessAsync(id);
        if (result.IsSuccess) return NoContent();
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpPost("{processId}/manual-details")]
    [HasPermission("payrollProcesses.create")]
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
    [HasPermission("payrollProcesses.view")]
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
    [HasPermission("payrollProcesses.delete")]
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

    [HttpGet("{id}/eligible-employees")]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<List<EligibleEmployeeResponseDto>>> GetEligibleEmployees(int id)
    {
        var result = await _payrollProcessingService.GetEligibleEmployeesAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost("{id}/add-employees")]
    [HasPermission("payrollProcesses.create")]
    public async Task<ActionResult> AddEmployees(int id, [FromBody] AddEmployeesRequestDto request)
    {
        var result = await _payrollProcessingService.AddEmployeesAsync(id, request.EmployeeIds);

        if (result.IsSuccess)
            return Ok(new { addedCount = result.Value });

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }

    [HttpGet("{id}/detail-summaries")]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<List<PayrollDetailSummaryResponseDto>>> GetDetailSummaries(int id)
    {
        var result = await _payrollProcessingService.GetDetailSummariesAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}/concept-summaries")]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<List<PayrollConceptSummaryResponseDto>>> GetConceptSummaries(int id)
    {
        var result = await _payrollProcessingService.GetConceptSummariesAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost("{id}/calculate")]
    [HasPermission("payrollProcesses.create")]
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

    [HttpPost("{id}/close")]
    [HasPermission("payrollProcesses.create")]
    public async Task<ActionResult<PayrollCloseResponseDto>> Close(int id)
    {
        var result = await _payrollProcessingService.CloseProcessAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }

    [HttpDelete("{processId}/employees/{employeeId}")]
    [HasPermission("payrollProcesses.delete")]
    public async Task<ActionResult> RemoveEmployee(int processId, int employeeId)
    {
        var result = await _payrollProcessingService.RemoveEmployeeFromProcessAsync(processId, employeeId);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(result.ErrorMessage);

        return StatusCode(500);
    }

    [HttpPost("{id}/close-and-pay")]
    [HasPermission("payrollProcesses.create")]
    public async Task<ActionResult<PayrollCloseAndPayResponseDto>> CloseAndPay(int id)
    {
        var result = await _payrollProcessingService.CloseAndPayAsync(id);

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

    [HttpGet("{processId}/receipt/{employeeId}")]
    [HasPermission("payrollProcesses.view")]
    public async Task<ActionResult<PayrollEmployeeReceiptDto>> GetEmployeeReceipt(int processId, int employeeId)
    {
        var result = await _payrollProcessingService.GetEmployeeReceiptAsync(processId, employeeId);

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
