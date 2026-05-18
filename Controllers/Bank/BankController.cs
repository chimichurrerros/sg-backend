using BackEnd.DTOs.Requests.Bank;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Bank;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Bank;

[Route("api/banks")]
[ApiController]
[Authorize]
public class BankController(BankService bankService) : ControllerBase
{
    private readonly BankService _bankService = bankService;

    [HttpGet]
    public async Task<ActionResult<ListBanksWrapperDto>> GetListBankes([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _bankService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListBanksWrapperDto>> GetAllBankes()
    {
        var result = await _bankService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankWrapperDto>> GetBankById(int id)
    {
        var result = await _bankService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<BankWrapperDto>> Create(BankRequestDto request)
    {
        var result = await _bankService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/banks/{result.Value!.Bank.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BankWrapperDto>> Update(int id, UpdateBankRequestDto request)
    {
        var result = await _bankService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> ToggleStatus(int id)
    {
        var result = await _bankService.ToggleStatusAsync(id);
        
        if (result.IsSuccess)
            return NoContent();
            
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
            
        return StatusCode(500);
    }
}
