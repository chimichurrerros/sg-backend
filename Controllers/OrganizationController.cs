using BackEnd.DTOs.Responses.Organization;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers;

[Route("api/organization")]
[ApiController]
[Authorize]
public class OrganizationController(OrganizationService organizationService) : ControllerBase
{
    private readonly OrganizationService _organizationService = organizationService;

    [HttpGet("department-boss")]
    [HasPermission("organizations.view")]
    public async Task<ActionResult<DepartmentBossResponseDto>> GetDepartmentBoss([FromQuery] int branchId, [FromQuery] int departmentId)
    {
        var result = await _organizationService.GetDepartmentBossAsync(branchId, departmentId);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorType.ToString() });

        return StatusCode(500);
    }

    [HttpGet("org-chart/{employeeId}")]
    [HasPermission("organizations.view")]
    public async Task<ActionResult<OrgChartResponseDto>> GetOrgChart(int employeeId, [FromQuery] int depth = 3)
    {
        var result = await _organizationService.GetOrgChartAsync(employeeId, depth);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return NotFound(new { message = result.ErrorMessage, code = result.ErrorType.ToString() });

        return StatusCode(500);
    }
}
