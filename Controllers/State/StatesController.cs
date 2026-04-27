using BackEnd.DTOs.Requests.State;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.State;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.State;

[Route("api/states")]
[ApiController]
[Authorize]
public class StatesController(StatesService statesService) : ControllerBase
{
    private readonly StatesService _statesService = statesService;

    [HttpGet]
    public async Task<ActionResult<ListStatesWrapperDto>> GetListStates([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _statesService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListStatesWrapperDto>> GetAllStates()
    {
        var result = await _statesService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StateWrapperDto>> GetStateById(int id)
    {
        var result = await _statesService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<StateWrapperDto>> Create(StateRequestDto request)
    {
        var result = await _statesService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/states/{result.Value!.State.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StateWrapperDto>> Update(int id, StateRequestDto request)
    {
        var result = await _statesService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _statesService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
