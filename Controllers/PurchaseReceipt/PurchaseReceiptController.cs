using BackEnd.DTOs.Requests.PurchaseReceipt;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.DTOs.Responses.PurchaseReceipt;
using BackEnd.Constants.Errors;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PurchaseReceipt;

[Route("api/purchase-receipts")]
[ApiController]
[Authorize]
public class PurchaseReceiptController(PurchaseReceiptService purchaseReceiptService) : ControllerBase
{
    private readonly PurchaseReceiptService _purchaseReceiptService = purchaseReceiptService;

    [HttpPost]
    [HasPermission("purchaseReceipts.create")]
    public async Task<ActionResult<BillWrapperDto>> ReceivePurchaseOrder(CreatePurchaseReceiptDto request)
    {
        var result = await _purchaseReceiptService.ReceivePurchaseOrderAsync(request);

        if (result.IsSuccess)
            return Created($"/api/bills/{result.Value!.Bill.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return this.HandleServerError(PurchaseReceiptError.ProcessFailed, result);
    }

    [HttpGet]
    public async Task<ActionResult<ListPurchaseReceiptsWrapperDto>> GetReceipts([FromQuery] PurchaseReceiptQueryDto queryDto)
    {
        var result = await _purchaseReceiptService.GetReceiptsAsync(queryDto);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseReceiptWrapperDto>> GetReceiptById(int id)
    {
        var result = await _purchaseReceiptService.GetReceiptByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("purchaseReceipts.view")]
    public async Task<ActionResult<ListBillsWrapperDto>> GetAll()
    {
        var result = await _purchaseReceiptService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }
}
