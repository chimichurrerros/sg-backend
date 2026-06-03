using BackEnd.DTOs.Requests.Schedule;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Schedule;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Schedule;

[Route("api/schedules")]
[ApiController]
[Authorize]
public class ScheduleController(ScheduleService scheduleService) : ControllerBase
{
    private readonly ScheduleService _scheduleService = scheduleService;

    [HttpGet]
    [HasPermission("schedules.view")]
    public async Task<ActionResult<ListSchedulesWrapperDto>> GetAll([FromQuery] OrganizationQueryDto query)
    {
        var result = await _scheduleService.GetAllAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("schedules.create")]
    public async Task<ActionResult<ScheduleWrapperDto>> Create(ScheduleRequestDto request)
    {
        var result = await _scheduleService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/schedules/{result.Value!.Schedule.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("schedules.update")]
    public async Task<ActionResult<ScheduleWrapperDto>> Update(int id, ScheduleRequestDto request)
    {
        var result = await _scheduleService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("schedules.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _scheduleService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
