using BackEnd.DTOs.Requests.SalesReturn;
using BackEnd.DTOs.Responses.SalesReturn;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Extensions;

namespace BackEnd.Controllers.SalesReturn;

[Route("api/sales-returns")]
[ApiController]
[Authorize]
public class SalesReturnController(SalesReturnService salesReturnService) : ControllerBase
{
    private readonly SalesReturnService _salesReturnService = salesReturnService;

    [HttpPost]
    public async Task<ActionResult<SalesReturnWrapperDto>> Create(CreateSalesReturnDto request)
    {
        var result = await _salesReturnService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/sales-returns/{result.Value!.SalesReturn.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListSalesReturnsWrapperDto>> GetAll()
    {
        var result = await _salesReturnService.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<ListSalesReturnsWrapperDto>> GetList([FromQuery] SalesReturnQueryDto queryDto)
    {
        var result = await _salesReturnService.GetListAsync(queryDto);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesReturnWrapperDto>> GetById(int id)
    {
        var result = await _salesReturnService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
