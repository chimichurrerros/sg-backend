using Microsoft.AspNetCore.Mvc;
using BackEnd.DTOs.Requests.Supplier;
using BackEnd.Services;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierCategoriesController(SupplierCategoryService supplierCategoryService) : ControllerBase
{
    private readonly SupplierCategoryService _service = supplierCategoryService;

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplierId(int supplierId)
    {
        var result = await _service.GetBySupplierIdAsync(supplierId);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupplierCategoryRequestDto request)
    {
        var result = await _service.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.IsSuccess ? NoContent() : NotFound(result);
    }
}