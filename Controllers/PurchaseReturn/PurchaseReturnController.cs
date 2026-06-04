using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseReturn;
using BackEnd.DTOs.Responses.PurchaseReturn;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PurchaseReturn;

[Route("api/purchase-returns")]
[ApiController]
[Authorize]
public class PurchaseReturnController(PurchaseReturnService purchaseReturnService) : ControllerBase
{
    private readonly PurchaseReturnService _purchaseReturnService = purchaseReturnService;

    [HttpGet("reasons")]
    [HasPermission("purchaseReturns.view")]
    public async Task<ActionResult<ListPurchaseReturnReasonsWrapperDto>> GetReasons()
    {
        var result = await _purchaseReturnService.GetReasonsAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpPost("reasons")]
    [HasPermission("purchaseReturns.create")]
    public async Task<ActionResult<PurchaseReturnReasonWrapperDto>> CreateReason(CreatePurchaseReturnReasonDto request)
    {
        var result = await _purchaseReturnService.CreateReasonAsync(request);

        if (result.IsSuccess)
            return Created($"/api/purchase-returns/reasons/{result.Value!.Reason.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpGet]
    [HasPermission("purchaseReturns.view")]
    public async Task<ActionResult<ListPurchaseReturnsWrapperDto>> GetList([FromQuery] PurchaseReturnQueryDto query)
    {
        var result = await _purchaseReturnService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("purchaseReturns.view")]
    public async Task<ActionResult<PurchaseReturnWrapperDto>> GetById(int id)
    {
        var result = await _purchaseReturnService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("purchaseReturns.create")]
    public async Task<ActionResult<PurchaseReturnWrapperDto>> Create(CreatePurchaseReturnDto request)
    {
        var result = await _purchaseReturnService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/purchase-returns/{result.Value!.PurchaseReturn.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return this.HandleServerError(PurchaseReturnError.ProcessFailed, result);
    }

    [HttpPost("with-bill")]
    [HasPermission("purchaseReturns.create")]
    public async Task<ActionResult<PurchaseReturnWrapperDto>> CreateWithBill(CreateBillAndReturnDto request)
    {
        var result = await _purchaseReturnService.CreateWithBillAsync(request);

        if (result.IsSuccess)
            return Created($"/api/purchase-returns/{result.Value!.PurchaseReturn.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return this.HandleServerError(PurchaseReturnError.ProcessFailed, result);
    }
}
