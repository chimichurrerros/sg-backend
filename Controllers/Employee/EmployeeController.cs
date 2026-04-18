using BackEnd.DTOs.Requests.Employee;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Employee;

[Route("api/employees")]
[ApiController]
[Authorize]
public class EmployeeController(EmployeeService employeeService) : ControllerBase
{
    private readonly EmployeeService _employeeService = employeeService;

    [HttpGet]
    public async Task<ActionResult<ListEmployeesWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _employeeService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeWrapperDto>> GetById(int id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeWrapperDto>> Create(CreateEmployeeRequestDto request)
    {
        var result = await _employeeService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/employees/{result.Value!.Employee.Id}", result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeWrapperDto>> Update(int id, UpdateEmployeeRequestDto request)
    {
        var result = await _employeeService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }
}
