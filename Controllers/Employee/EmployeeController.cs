using BackEnd.DTOs.Requests.Employee;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Employee;

[Route("api/employees")]
[ApiController]
[Authorize]
public class EmployeeController(EmployeeService employeeService) : ControllerBase
{
    private readonly EmployeeService _employeeService = employeeService;

    [HttpGet]
    [HasPermission("employees.view")]
    public async Task<ActionResult<ListEmployeesWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _employeeService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("employees.view")]
    public async Task<ActionResult<EmployeeWrapperDto>> GetById(int id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("employees.create")]
    public async Task<ActionResult<EmployeeWrapperDto>> Create(CreateEmployeeRequestDto request)
    {
        var result = await _employeeService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/employees/{result.Value!.Employee.Id}", result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("employees.update")]
    public async Task<ActionResult<EmployeeWrapperDto>> Update(int id, UpdateEmployeeRequestDto request)
    {
        var result = await _employeeService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("employees.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _employeeService.DeleteAsync(id);
        if (result.IsSuccess) return NoContent();
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpGet("{id}/position-history")]
    [HasPermission("employees.view")]
    public async Task<ActionResult<ListEmployeePositionHistoriesWrapperDto>> GetPositionHistory(int id)
    {
        var result = await _employeeService.GetPositionHistoriesAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost("{id}/position-history")]
    [HasPermission("employees.create")]
    public async Task<ActionResult<EmployeePositionHistoryWrapperDto>> AddPositionHistory(int id, CreateEmployeePositionHistoryRequestDto request)
    {
        var result = await _employeeService.AddPositionHistoryAsync(id, request);
        if (result.IsSuccess) return Created($"/api/employees/{id}/position-history/{result.Value!.History.Id}", result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}/position-history/{historyId}")]
    [HasPermission("employees.update")]
    public async Task<ActionResult<EmployeePositionHistoryWrapperDto>> UpdatePositionHistory(int id, int historyId, UpdateEmployeePositionHistoryRequestDto request)
    {
        var result = await _employeeService.UpdatePositionHistoryAsync(id, historyId, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpDelete("{id}/position-history/{historyId}")]
    [HasPermission("employees.delete")]
    public async Task<ActionResult> DeletePositionHistory(int id, int historyId)
    {
        var result = await _employeeService.DeletePositionHistoryAsync(id, historyId);
        if (result.IsSuccess) return NoContent();
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpGet("{id}/relations")]
    [HasPermission("employees.view")]
    public async Task<ActionResult<ListEmployeeRelationsWrapperDto>> GetRelations(int id)
    {
        var result = await _employeeService.GetRelationsAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost("{id}/relations")]
    [HasPermission("employees.create")]
    public async Task<ActionResult<EmployeeRelationWrapperDto>> AddRelation(int id, CreateEmployeeRelationRequestDto request)
    {
        var result = await _employeeService.AddRelationAsync(id, request);
        if (result.IsSuccess) return Created($"/api/employees/{id}/relations/{result.Value!.Relation.Id}", result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}/relations/{relationId}")]
    [HasPermission("employees.update")]
    public async Task<ActionResult<EmployeeRelationWrapperDto>> UpdateRelation(int id, int relationId, UpdateEmployeeRelationRequestDto request)
    {
        var result = await _employeeService.UpdateRelationAsync(id, relationId, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }

    [HttpDelete("{id}/relations/{relationId}")]
    [HasPermission("employees.delete")]
    public async Task<ActionResult> DeleteRelation(int id, int relationId)
    {
        var result = await _employeeService.DeleteRelationAsync(id, relationId);
        if (result.IsSuccess) return NoContent();
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Conflict) return Conflict(result.ErrorMessage);
        return StatusCode(500);
    }
}
