using BackEnd.DTOs.Requests.ProductCategory;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.ProductCategory;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Product;

[Route("api/product-categories")]
[ApiController]
[Authorize]
public class ProductCategoriesController(ProductCategoriesService productCategoriesService) : ControllerBase
{
    private readonly ProductCategoriesService _productCategoriesService = productCategoriesService;

    [HttpGet]
    [HasPermission("productCategories.view")]
    public async Task<ActionResult<ListProductCategoriesWrapperDto>> GetListProductCategories([FromQuery] ProductCategoryQueryDto query)
    {
        var result = await _productCategoriesService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("productCategories.view")]
    public async Task<ActionResult<ListProductCategoriesWrapperDto>> GetAllProductCategories()
    {
        var result = await _productCategoriesService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("productCategories.view")]
    public async Task<ActionResult<ProductCategoryWrapperDto>> GetProductCategoryById(int id)
    {
        var result = await _productCategoriesService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("productCategories.create")]
    public async Task<ActionResult<ProductCategoryWrapperDto>> Create(ProductCategoryRequestDto request)
    {
        var result = await _productCategoriesService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/product-categories/{result.Value!.ProductCategory.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("productCategories.update")]
    public async Task<ActionResult<ProductCategoryWrapperDto>> Update(int id, ProductCategoryRequestDto request)
    {
        var result = await _productCategoriesService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("productCategories.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productCategoriesService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
