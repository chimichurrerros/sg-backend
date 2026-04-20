using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.ProductBrand;
using BackEnd.DTOs.Responses.ProductBrand;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Product;

/// <summary>
/// Controller for product-brand operations.
/// This controller only handles HTTP concerns:
/// 1) Receives and validates requests
/// 2) Delegates business logic to ProductBrandsService
/// 3) Translates service results into HTTP responses
/// </summary>
[Route("api/product-brands")]
[ApiController]
[Authorize]
public class ProductBrandController(ProductBrandsService productBrandsService) : ControllerBase
{
    private readonly ProductBrandsService _productBrandsService = productBrandsService;

    /// <summary>
    /// GET /api/product-brands
    /// Retrieves a paginated list of product brands.
    /// </summary>
    /// <param name="pagination">Pagination parameters from query string (Page, PageSize)</param>
    /// <returns>Paginated list of product brands and pagination metadata</returns>
    /// <response code="200">Returns the paginated list</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListProductBrandsWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListProductBrandsWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _productBrandsService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    /// <summary>
    /// GET /api/product-brands/{id}
    /// Retrieves a single product brand by its identifier.
    /// </summary>
    /// <param name="id">Product brand identifier</param>
    /// <returns>Product brand data</returns>
    /// <response code="200">Returns the requested product brand</response>
    /// <response code="404">Product brand not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductBrandWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductBrandWrapperDto>> GetById(int id)
    {
        var result = await _productBrandsService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// POST /api/product-brands
    /// Creates a new product brand.
    /// </summary>
    /// <param name="request">Product brand data</param>
    /// <returns>Created product brand</returns>
    /// <response code="201">Product brand created successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductBrandWrapperDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductBrandWrapperDto>> Create([FromBody] ProductBrandRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productBrandsService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/product-brands/{result.Value!.ProductBrand.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// PUT /api/product-brands/{id}
    /// Replaces an existing product brand.
    /// </summary>
    /// <param name="id">Product brand identifier</param>
    /// <param name="request">Updated product brand data</param>
    /// <returns>Updated product brand</returns>
    /// <response code="200">Product brand updated successfully</response>
    /// <response code="400">Invalid request payload</response>
    /// <response code="404">Product brand not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductBrandWrapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductBrandWrapperDto>> Update(int id, [FromBody] ProductBrandRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productBrandsService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    /// <summary>
    /// DELETE /api/product-brands/{id}
    /// Deletes a product brand by its identifier.
    /// </summary>
    /// <param name="id">Product brand identifier</param>
    /// <returns>No content when deletion is successful</returns>
    /// <response code="204">Product brand deleted successfully</response>
    /// <response code="404">Product brand not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productBrandsService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
