using BackEnd.DTOs.Requests.PayrollUpdate;
using BackEnd.DTOs.Responses.PayrollUpdate;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Payroll;

[Route("api/payroll-updates")]
[ApiController]
[Authorize]
public class PayrollUpdatesController(PayrollUpdateService payrollUpdateService) : ControllerBase
{
    private readonly PayrollUpdateService _payrollUpdateService = payrollUpdateService;

    [HttpGet]
    public async Task<ActionResult<List<PayrollUpdateResponseDto>>> GetList()
    {
        var result = await _payrollUpdateService.GetListAsync();
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<PayrollUpdateResponseDto>> Create(PayrollUpdateCreateDto request)
    {
        var result = await _payrollUpdateService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/payroll-updates/{result.Value!.Id}", result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PayrollUpdateResponseDto>> Update(int id, PayrollUpdateCreateDto request)
    {
        var result = await _payrollUpdateService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }
}