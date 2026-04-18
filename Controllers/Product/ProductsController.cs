using BackEnd.DTOs.Requests.Product;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Product;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Product;

[Route("api/products")]
[ApiController]
[Authorize]
public class ProductsController(ProductsService productsService) : ControllerBase
{
    private readonly ProductsService _productsService = productsService;

    [HttpGet]
    public async Task<ActionResult<ListProductsWrapperDto>> GetListProducts([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _productsService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListProductsWrapperDto>> GetAllProducts()
    {
        var result = await _productsService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductWrapperDto>> GetProductById(int id)
    {
        var result = await _productsService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<ProductWrapperDto>> Create(ProductRequestDto request)
    {
        var result = await _productsService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/products/{result.Value!.Product.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductWrapperDto>> Update(int id, ProductRequestDto request)
    {
        var result = await _productsService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
