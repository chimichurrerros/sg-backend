using BackEnd.DTOs.Requests.Product;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Product;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Product;

[Route("api/products")]
[ApiController]
[Authorize]
public class ProductsController(ProductsService productsService) : ControllerBase
{
    private readonly ProductsService _productsService = productsService;

    [HttpGet]
    [HasPermission("products.view")]
    public async Task<ActionResult<ListProductsWrapperDto>> GetListProducts([FromQuery] ProductQueryDto query)
    {
        var result = await _productsService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("products.view")]
    public async Task<ActionResult<ListProductsWrapperDto>> GetAllProducts()
    {
        var result = await _productsService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("products.view")]
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
    [HasPermission("products.create")]
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
    [HasPermission("products.update")]
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
    [HasPermission("products.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}/suppliers")]
    [HasPermission("products.view")]
    public async Task<ActionResult<ProductWrapperDto>> GetSuppliers(int id)
    {
        var result = await _productsService.GetAllSuppliers(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("by-branch/{branchId}")]
    [HasPermission("products.view")]
    public async Task<ActionResult<ListProductsStockWrapperDto>> GetProductsByBranch(int branchId)
    {
        var result = await _productsService.GetByBranchIdAsync(branchId);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }
}
