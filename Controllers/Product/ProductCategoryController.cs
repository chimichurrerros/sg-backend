using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.ProductCategory;
using BackEnd.DTOs.Responses.ProductCategory;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Product;

/// <summary>
/// Controller for product-category operations.
/// This controller only handles HTTP concerns:
/// 1) Receives and validates requests
/// 2) Delegates business logic to ProductCategoriesService
/// 3) Translates service results into HTTP responses
/// </summary>
[Route("api/product-categories")]
[ApiController]
[Authorize]
public class ProductCategoryController(ProductCategoriesService productCategoriesService) : ControllerBase
{
    private readonly ProductCategoriesService _productCategoriesService = productCategoriesService;

    /// <summary>
    /// GET /api/product-categories
    /// Retrieves a paginated list of product categories.
    /// </summary>
    /// <param name="pagination">Pagination parameters from query string (Page, PageSize)</param>
    /// <returns>Paginated list of product categories and pagination metadata</returns>
    /// <response code="200">Returns the paginated list</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListProductCategoriesWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListProductCategoriesWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _productCategoriesService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    /// <summary>
    /// GET /api/product-categories/{id}
    /// Retrieves a single product category by its identifier.
    /// </summary>
    /// <param name="id">Product category identifier</param>
    /// <returns>Product category data</returns>
    /// <response code="200">Returns the requested product category</response>
    /// <response code="404">Product category not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductCategoryWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductCategoryWrapperDto>> GetById(int id)
    {
        var result = await _productCategoriesService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// POST /api/product-categories
    /// Creates a new product category.
    /// </summary>
    /// <param name="request">Product category data</param>
    /// <returns>Created product category</returns>
    /// <response code="201">Product category created successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductCategoryWrapperDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductCategoryWrapperDto>> Create([FromBody] ProductCategoryRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productCategoriesService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/product-categories/{result.Value!.ProductCategory.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// PUT /api/product-categories/{id}
    /// Replaces an existing product category.
    /// </summary>
    /// <param name="id">Product category identifier</param>
    /// <param name="request">Updated product category data</param>
    /// <returns>Updated product category</returns>
    /// <response code="200">Product category updated successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="404">Product category not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductCategoryWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductCategoryWrapperDto>> Update(int id, [FromBody] ProductCategoryRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productCategoriesService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// DELETE /api/product-categories/{id}
    /// Deletes a product category by its identifier.
    /// </summary>
    /// <param name="id">Product category identifier</param>
    /// <returns>No content when deletion is successful</returns>
    /// <response code="204">Product category deleted successfully</response>
    /// <response code="404">Product category not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productCategoriesService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
