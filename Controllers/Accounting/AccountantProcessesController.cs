using BackEnd.DTOs.Requests.AccountantProcess;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.AccountantProcess;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Accounting;

[Route("api/accountant-processes")]
[ApiController]
[Authorize]
public class AccountantProcessesController(AccountantProcessService accountantProcessService) : ControllerBase
{
    private readonly AccountantProcessService _accountantProcessService = accountantProcessService;

    [HttpGet]
    public async Task<ActionResult<ListAccountantProcessesWrapperDto>> GetList([FromQuery] PaginationRequestDto query)
    {
        var result = await _accountantProcessService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListAccountantProcessesWrapperDto>> GetAll()
    {
        var result = await _accountantProcessService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("last")]
    public async Task<ActionResult<AccountantProcessWrapperDto>> GetLast()
    {
        var result = await _accountantProcessService.GetLastAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountantProcessWrapperDto>> GetById(int id)
    {
        var result = await _accountantProcessService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<AccountantProcessWrapperDto>> Create(CreateAccountantProcessRequestDto request)
    {
        var result = await _accountantProcessService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/accountant-processes/{result.Value!.AccountantProcess.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AccountantProcessWrapperDto>> Update(int id, UpdateAccountantProcessRequestDto request)
    {
        var result = await _accountantProcessService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return StatusCode(500);
    }
}
