using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.PurchaseOrder;

[Route("api/purchaseorders")]
[ApiController]
[Authorize]
public class PurchaseOrderController(PurchaseOrderService purchaseOrderService) : ControllerBase
{
    private readonly PurchaseOrderService _purchaseOrderService = purchaseOrderService;

    [HttpGet]
    public async Task<ActionResult<ListPurchaseOrdersWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _purchaseOrderService.GetListAsync(pagination);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListPurchaseOrdersWrapperDto>> GetAll()
    {
        var result = await _purchaseOrderService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("draft/{purchaseRequestId}")]
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

    [HttpPut("{id}")]
    public async Task<ActionResult<PurchaseOrderWrapperDto>> Update(int id, UpdatePurchaseOrderRequestDto request)
    {
        var result = await _purchaseOrderService.UpdateAsync(id, request);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }
}
