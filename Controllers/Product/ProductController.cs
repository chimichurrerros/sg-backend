using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Product;
using BackEnd.DTOs.Responses.Product;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Product;

/// <summary>
/// Controller for product operations.
/// This controller only handles HTTP concerns:
/// 1) Receives and validates requests
/// 2) Delegates business logic to ProductsService
/// 3) Translates service results into HTTP responses
/// </summary>
[Route("api/products")]
[ApiController]
[Authorize]
public class ProductController(ProductsService productsService) : ControllerBase
{
    private readonly ProductsService _productsService = productsService;

    /// <summary>
    /// GET /api/products
    /// Retrieves a paginated list of products.
    /// </summary>
    /// <param name="pagination">Pagination parameters from query string (Page, PageSize)</param>
    /// <returns>Paginated list of products and pagination metadata</returns>
    /// <response code="200">Returns the paginated list</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListProductsWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListProductsWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _productsService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    /// <summary>
    /// GET /api/products/{id}
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>Product data</returns>
    /// <response code="200">Returns the requested product</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductWrapperDto>> GetById(int id)
    {
        var result = await _productsService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// POST /api/products
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product data</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductWrapperDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductWrapperDto>> Create([FromBody] ProductRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productsService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/products/{result.Value!.Product.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// PUT /api/products/{id}
    /// Replaces an existing product.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="request">Updated product data</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductWrapperDto>> Update(int id, [FromBody] ProductRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productsService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// DELETE /api/products/{id}
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <returns>No content when deletion is successful</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
