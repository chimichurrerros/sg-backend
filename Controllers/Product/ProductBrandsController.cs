using BackEnd.DTOs.Requests.ProductBrand;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.ProductBrand;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Product;

[Route("api/product-brands")]
[ApiController]
[Authorize]
public class ProductBrandsController(ProductBrandsService productBrandsService) : ControllerBase
{
    private readonly ProductBrandsService _productBrandsService = productBrandsService;

    [HttpGet]
    [HasPermission("productBrands.view")]
    public async Task<ActionResult<ListProductBrandsWrapperDto>> GetListProductBrands([FromQuery] ProductBrandQueryDto query)
    {
        var result = await _productBrandsService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("productBrands.view")]
    public async Task<ActionResult<ListProductBrandsWrapperDto>> GetAllProductBrands()
    {
        var result = await _productBrandsService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("productBrands.view")]
    public async Task<ActionResult<ProductBrandWrapperDto>> GetProductBrandById(int id)
    {
        var result = await _productBrandsService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("productBrands.create")]
    public async Task<ActionResult<ProductBrandWrapperDto>> Create(ProductBrandRequestDto request)
    {
        var result = await _productBrandsService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/product-brands/{result.Value!.ProductBrand.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("productBrands.update")]
    public async Task<ActionResult<ProductBrandWrapperDto>> Update(int id, ProductBrandRequestDto request)
    {
        var result = await _productBrandsService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("productBrands.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productBrandsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
