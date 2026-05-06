using Microsoft.AspNetCore.Mvc;
using BackEnd.DTOs.Requests.Bank.BankMovement;
using BackEnd.Services.Interfaces;
using BackEnd.Utils;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankMovementsController : ControllerBase
{
    private readonly IBankMovementService _service;

    public BankMovementsController(IBankMovementService service)
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
    public async Task<IActionResult> Create([FromBody] BankMovementRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}