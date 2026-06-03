using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PurchaseOrderForSupplier;

[Route("api/purchaseorders-for-supplier")]
[ApiController]
[Authorize]
public class PurchaseOrderForSupplierController(PurchaseOrderForSupplierService purchaseOrderForSupplierService) : ControllerBase
{
    private readonly PurchaseOrderForSupplierService _purchaseOrderForSupplierService = purchaseOrderForSupplierService;

    [HttpGet]
    [HasPermission("purchaseOrderForSuppliers.view")]
    public async Task<ActionResult<ListPurchaseOrdersForSupplierWrapperDto>> GetList([FromQuery] PurchaseOrderForSupplierQueryDto query)
    {
        var result = await _purchaseOrderForSupplierService.GetListAsync(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("purchaseOrderForSuppliers.view")]
    public async Task<ActionResult<ListPurchaseOrdersForSupplierWrapperDto>> GetAll()
    {
        var result = await _purchaseOrderForSupplierService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("purchaseOrderForSuppliers.view")]
    public async Task<ActionResult<PurchaseOrderForSupplierWrapperDto>> GetById(int id)
    {
        var result = await _purchaseOrderForSupplierService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}/state")]
    [HasPermission("purchaseOrderForSuppliers.update")]
    public async Task<ActionResult> UpdateState(int id, UpdatePurchaseOrderForSupplierStateDto request)
    {
        var result = await _purchaseOrderForSupplierService.UpdateStateAsync(id, request);
        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }
}
