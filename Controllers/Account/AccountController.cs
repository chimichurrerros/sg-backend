using Microsoft.AspNetCore.Mvc;
using BackEnd.DTOs.Requests.Accounts;
using BackEnd.Services.Interfaces;
using BackEnd.Utils;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.NotFound ? NotFound(result) : BadRequest(result);
        }
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountRequestDto request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.NotFound ? NotFound(result) : BadRequest(result);
        }
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.NotFound ? NotFound(result) : BadRequest(result);
        }
        return NoContent(); // 204 No Content es el estándar para un Delete exitoso
    }
}