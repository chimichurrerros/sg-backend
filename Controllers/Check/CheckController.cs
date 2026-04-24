using Microsoft.AspNetCore.Mvc;
using BackEnd.DTOs.Requests.Checks;
using BackEnd.Services.Interfaces;
using BackEnd.Utils;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChecksController : ControllerBase
{
    private readonly ICheckService _service;

    public ChecksController(ICheckService service)
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
    public async Task<IActionResult> Create([FromBody] CreateCheckRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    // Usamos PATCH porque es una actualización parcial (solo estado y fecha)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCheckStatusRequestDto request)
    {
        var result = await _service.UpdateStatusAsync(id, request);
        if (!result.IsSuccess)
        {
            return result.ErrorType == ErrorType.NotFound ? NotFound(result) : BadRequest(result);
        }
        return Ok(result.Value);
    }
}