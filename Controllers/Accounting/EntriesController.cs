using BackEnd.DTOs.Requests.Entry;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Entry;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Accounting;

[Route("api/entries")]
[ApiController]
[Authorize]
public class EntriesController(EntryService entryService) : ControllerBase
{
    private readonly EntryService _entryService = entryService;

    [HttpGet]
    public async Task<ActionResult<ListEntriesWrapperDto>> GetList([FromQuery] PaginationRequestDto query)
    {
        var result = await _entryService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListEntriesWrapperDto>> GetAll()
    {
        var result = await _entryService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntryWrapperDto>> GetById(int id)
    {
        var result = await _entryService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<EntryWrapperDto>> Create(CreateEntryRequestDto request)
    {
        var result = await _entryService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/entries/{result.Value!.Entry.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EntryWrapperDto>> Update(int id, UpdateEntryRequestDto request)
    {
        var result = await _entryService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _entryService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }
}
