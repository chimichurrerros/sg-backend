using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PurchaseOrder;

[Route("api/purchaseorders")]
[ApiController]
[Authorize]
public class PurchaseOrderController(PurchaseOrderService purchaseOrderService) : ControllerBase
{
    private readonly PurchaseOrderService _purchaseOrderService = purchaseOrderService;

    [HttpGet]
    [HasPermission("purchaseOrders.view")]
    public async Task<ActionResult<ListPurchaseOrdersWrapperDto>> GetList([FromQuery] PurchaseOrderQueryDto query)
    {
        var result = await _purchaseOrderService.GetListAsync(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("purchaseOrders.view")]
    public async Task<ActionResult<ListPurchaseOrdersWrapperDto>> GetAll()
    {
        var result = await _purchaseOrderService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("draft/{purchaseRequestId}")]
    [HasPermission("purchaseOrders.view")]
    public async Task<ActionResult<PurchaseOrderDraftWrapperDto>> GetDraft(int purchaseRequestId)
    {
        var result = await _purchaseOrderService.GetDraftByPurchaseRequestIdAsync(purchaseRequestId);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("purchaseOrders.view")]
    public async Task<ActionResult<PurchaseOrderWrapperDto>> GetById(int id)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("purchaseOrders.create")]
    public async Task<ActionResult<PurchaseOrderWrapperDto>> Create(CreatePurchaseOrderRequestDto request)
    {
        var result = await _purchaseOrderService.CreateAsync(request);
        if (result.IsSuccess)
            return Created($"/api/purchaseorders/{result.Value!.PurchaseOrder.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}/cancel")]
    [HasPermission("purchaseOrders.update")]
    public async Task<ActionResult> Cancel(int id)
    {
        var result = await _purchaseOrderService.CancelMainOrderAsync(id);
        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
