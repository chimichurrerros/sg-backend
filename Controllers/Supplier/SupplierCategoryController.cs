using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers;

[Route("api/suppliercategories")]
[ApiController]
[Authorize]
public class SupplierCategoriesController(SupplierCategoryService supplierCategoryService) : ControllerBase
{
    private readonly SupplierCategoryService _service = supplierCategoryService;

    [HttpGet("supplier/{supplierId}")]
    [HasPermission("supplierCategories.view")]
    public async Task<ActionResult<SupplierCategoryWrapperDto>> GetBySupplierId(int supplierId)
    {
        var result = await _service.GetBySupplierIdAsync(supplierId);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("supplierCategories.create")]
    public async Task<ActionResult<SupplierCategoryResponseDto>> Create([FromBody] SupplierCategoryRequestDto request)
    {
        var result = await _service.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/suppliercategories/supplier/{result.Value!.SupplierCategory.SupplierId}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("supplierCategories.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
