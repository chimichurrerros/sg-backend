using BackEnd.DTOs.Requests.Stock;
using BackEnd.DTOs.Responses.Stock;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Stock;

[Route("api/stock")]
[ApiController]
[Authorize]
public class StockController(StockService stockService) : ControllerBase
{
    private readonly StockService _stockService = stockService;

    [HttpGet]
    [HasPermission("stock.view")]
    public async Task<ActionResult<ListStocksWrapperDto>> GetListStocks([FromQuery] StockQueryDto query)
    {
        var result = await _stockService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<ListStocksWrapperDto>> GetAllStocks()
    {
        var result = await _stockService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<StockWrapperDto>> GetStockById(int id)
    {
        var result = await _stockService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("stock.create")]
    public async Task<ActionResult<StockWrapperDto>> Create(StockRequestDto request)
    {
        var result = await _stockService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/stock/{result.Value!.Stock.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("stock.update")]
    public async Task<ActionResult<StockWrapperDto>> Update(int id, StockRequestDto request)
    {
        var result = await _stockService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("stock.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _stockService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
