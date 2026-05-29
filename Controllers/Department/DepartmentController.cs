using BackEnd.DTOs.Requests.Department;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Department;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Department;

[Route("api/departments")]
[ApiController]
[Authorize]
public class DepartmentController(DepartmentService departmentService) : ControllerBase
{
    private readonly DepartmentService _departmentService = departmentService;

    [HttpGet]
    public async Task<ActionResult<ListDepartmentsWrapperDto>> GetAll([FromQuery] OrganizationQueryDto query)
    {
        var result = await _departmentService.GetAllAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentWrapperDto>> Create(DepartmentRequestDto request)
    {
        var result = await _departmentService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/departments/{result.Value!.Department.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentWrapperDto>> Update(int id, DepartmentRequestDto request)
    {
        var result = await _departmentService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}