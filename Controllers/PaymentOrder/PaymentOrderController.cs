using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PaymentOrder;
using BackEnd.DTOs.Responses.PaymentOrder;
using BackEnd.DTOs.Responses.PurchaseReturn;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PaymentOrder;

[Route("api/payment-orders")]
[ApiController]
[Authorize]
public class PaymentOrderController(PaymentOrderService paymentOrderService, BackEnd.Services.PurchaseReturnService purchaseReturnService) : ControllerBase
{
    private readonly PaymentOrderService _paymentOrderService = paymentOrderService;
    private readonly BackEnd.Services.PurchaseReturnService _purchaseReturnService = purchaseReturnService;

    [HttpPost]
    [HasPermission("paymentOrders.create")]
    public async Task<ActionResult<PaymentOrderWrapperDto>> Create(CreatePaymentOrderDto request)
    {
        var result = await _paymentOrderService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/payment-orders/{result.Value!.PaymentOrder.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("paymentOrders.view")]
    public async Task<ActionResult<PaymentOrderWrapperDto>> GetById(int id)
    {
        var result = await _paymentOrderService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet]
    [HasPermission("paymentOrders.view")]
    public async Task<ActionResult<ListPaymentOrdersWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _paymentOrderService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost("{id}/process")]
    [HasPermission("paymentOrders.create")]
    public async Task<ActionResult<PaymentOrderWrapperDto>> Process(int id, ProcessPaymentOrderDto request)
    {
        request.PaymentOrderId = id;

        var result = await _paymentOrderService.ProcessPaymentAsync(request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost("receive")]
    [HasPermission("paymentOrders.create")]
    public async Task<ActionResult<PurchaseReturnWrapperDto>> ReceiveBillAndReturn([FromBody] BackEnd.DTOs.Requests.PurchaseReturn.CreateBillAndReturnDto request)
    {
        var result = await _purchaseReturnService.CreateWithBillAsync(request);

        if (result.IsSuccess)
            return Created($"/api/purchase-returns/{result.Value!.PurchaseReturn.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
