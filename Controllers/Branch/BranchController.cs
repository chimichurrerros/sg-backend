using BackEnd.DTOs.Requests.Branch;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Branch;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Branch;

[Route("api/branches")]
[ApiController]
[Authorize]
public class BranchController(BranchService branchService) : ControllerBase
{
    private readonly BranchService _branchService = branchService;

    [HttpGet]
    public async Task<ActionResult<ListBranchesWrapperDto>> GetListBranches([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _branchService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListBranchesWrapperDto>> GetAllBranches()
    {
        var result = await _branchService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BranchWrapperDto>> GetBranchById(int id)
    {
        var result = await _branchService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<BranchWrapperDto>> Create(BranchRequestDto request)
    {
        var result = await _branchService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/branches/{result.Value!.Branch.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BranchWrapperDto>> Update(int id, BranchRequestDto request)
    {
        var result = await _branchService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _branchService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
