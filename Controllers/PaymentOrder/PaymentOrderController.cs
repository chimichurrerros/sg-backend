using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PaymentOrder;
using BackEnd.DTOs.Responses.PaymentOrder;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.PaymentOrder;

[Route("api/payment-orders")]
[ApiController]
[Authorize]
public class PaymentOrderController(PaymentOrderService paymentOrderService) : ControllerBase
{
    private readonly PaymentOrderService _paymentOrderService = paymentOrderService;

    [HttpPost]
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
    public async Task<ActionResult<ListPaymentOrdersWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _paymentOrderService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost("{id}/process")]
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
}
