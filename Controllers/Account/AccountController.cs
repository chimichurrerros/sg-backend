using BackEnd.DTOs.Requests.Accounts;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Accounts;
using BackEnd.Extensions;
using BackEnd.Infrastructure.Authorization;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Account;

[Route("api/accounts")]
[ApiController]
[Authorize]
public class AccountsController(AccountService accountService) : ControllerBase
{
    private readonly AccountService _accountService = accountService;

    [HttpGet]
    [HasPermission("accounts.view")]
    public async Task<ActionResult<ListAccountsWrapperDto>> GetListAccounts([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _accountService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("accounts.view")]
    public async Task<ActionResult<ListAccountsWrapperDto>> GetAllAccounts()
    {
        var result = await _accountService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("accounts.view")]
    public async Task<ActionResult<AccountWrapperDto>> GetById(int id)
    {
        var result = await _accountService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("accounts.create")]
    public async Task<ActionResult<AccountWrapperDto>> Create(CreateAccountRequestDto request)
    {
        var result = await _accountService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/accounts/{result.Value!.Account.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("accounts.update")]
    public async Task<ActionResult<AccountWrapperDto>> Update(int id, UpdateAccountRequestDto request)
    {
        var result = await _accountService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("accounts.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.ToggleStatusAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.NotFound ? NotFound(result) : BadRequest(result);
        }
        return NoContent(); // 204 No Content es el estándar para un Delete exitoso
    }
}