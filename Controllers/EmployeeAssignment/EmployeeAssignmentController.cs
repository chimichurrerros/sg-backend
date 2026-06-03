using BackEnd.DTOs.Requests.Employee;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.EmployeeAssignment;

[Route("api/employee-assignments")]
[ApiController]
[Authorize]
public class EmployeeAssignmentController(IEmployeeAssignmentService employeeAssignmentService) : ControllerBase
{
    private readonly IEmployeeAssignmentService _employeeAssignmentService = employeeAssignmentService;

    [HttpPost]
    [HasPermission("employeeAssignments.create")]
    public async Task<ActionResult<PositionByScheduleByEmployeeDto>> Assign(PositionByScheduleByEmployeeDto dto)
    {
        dto.Id = 0;

        var result = await _employeeAssignmentService.AssignPositionAndScheduleAsync(dto);
        if (result.IsSuccess)
            return Created($"/api/employee-assignments/{result.Value!.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("employeeAssignments.update")]
    public async Task<ActionResult<PositionByScheduleByEmployeeDto>> Update(int id, PositionByScheduleByEmployeeDto dto)
    {
        dto.Id = id;

        var result = await _employeeAssignmentService.AssignPositionAndScheduleAsync(dto);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }
}
