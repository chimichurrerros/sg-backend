using BackEnd.DTOs.Requests.PurchaseReceipt;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.Constants.Errors;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.PurchaseReceipt;

[Route("api/purchase-receipts")]
[ApiController]
[Authorize]
public class PurchaseReceiptController(PurchaseReceiptService purchaseReceiptService) : ControllerBase
{
    private readonly PurchaseReceiptService _purchaseReceiptService = purchaseReceiptService;

    [HttpPost]
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

    [HttpGet("all")]
    public async Task<ActionResult<ListBillsWrapperDto>> GetAll()
    {
        var result = await _purchaseReceiptService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }
}
