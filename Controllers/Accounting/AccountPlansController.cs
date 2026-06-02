using BackEnd.DTOs.Requests.AccountPlan;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.AccountPlan;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Accounting;

[Route("api/account-plans")]
[ApiController]
[Authorize]
public class AccountPlansController(AccountPlanService accountPlanService) : ControllerBase
{
    private readonly AccountPlanService _accountPlanService = accountPlanService;

    [HttpGet]
    public async Task<ActionResult<ListAccountPlansWrapperDto>> GetList([FromQuery] PaginationRequestDto query)
    {
        var result = await _accountPlanService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListAccountPlansWrapperDto>> GetAll()
    {
        var result = await _accountPlanService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountPlanWrapperDto>> GetById(int id)
    {
        var result = await _accountPlanService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<AccountPlanWrapperDto>> Create(CreateAccountPlanRequestDto request)
    {
        var result = await _accountPlanService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/account-plans/{result.Value!.AccountPlan.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AccountPlanWrapperDto>> Update(int id, UpdateAccountPlanRequestDto request)
    {
        var result = await _accountPlanService.UpdateAsync(id, request);

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
        var result = await _accountPlanService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }
}
