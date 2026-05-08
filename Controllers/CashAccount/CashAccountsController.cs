using BackEnd.DTOs.Requests.CashAccount;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.CashAccount;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.CashAccount;

[Route("api/cash-accounts")]
[ApiController]
[Authorize]
public class CashAccountsController(CashAccountsService cashAccountsService) : ControllerBase
{
    private readonly CashAccountsService _cashAccountsService = cashAccountsService;

    [HttpGet]
    public async Task<ActionResult<ListCashAccountsWrapperDto>> GetListCashAccounts([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _cashAccountsService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListCashAccountsWrapperDto>> GetAllCashAccounts()
    {
        var result = await _cashAccountsService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CashAccountWrapperDto>> GetCashAccountById(int id)
    {
        var result = await _cashAccountsService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<CashAccountWrapperDto>> Create(CashAccountRequestDto request)
    {
        var result = await _cashAccountsService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/cash-accounts/{result.Value!.CashAccount.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CashAccountWrapperDto>> Update(int id, CashAccountRequestDto request)
    {
        var result = await _cashAccountsService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _cashAccountsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
