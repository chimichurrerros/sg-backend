using BackEnd.DTOs.Requests.PurchaseRequest;
using BackEnd.DTOs.Responses.PurchaseRequest;
using BackEnd.Constants.Errors;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.PurchaseRequest;

[Route("api/purchase-requests")]
[ApiController]
[Authorize]
public class PurchaseRequestController(PurchaseRequestService purchaseRequestService) : ControllerBase
{
    private readonly PurchaseRequestService _purchaseRequestService = purchaseRequestService;

    [HttpPost]
    [HasPermission("purchaseRequests.create")]
    public async Task<ActionResult<PurchaseRequestWrapperDto>> Create(CreatePurchaseRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var result = await _purchaseRequestService.CreateAsync(request, userId);
        if (result.IsSuccess) return Created($"/api/purchase-requests/{result.Value!.PurchaseRequest.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return this.HandleServerError(PurchaseRequestError.ProcessFailed, result);
    }

    [HttpGet("all")]
    [HasPermission("purchaseRequests.view")]
    public async Task<ActionResult<ListPurchaseRequestsWrapperDto>> GetAll()
    {
        var result = await _purchaseRequestService.GetAllAsync();
        if (result.IsSuccess) return Ok(result.Value);

        return this.HandleServerError(PurchaseRequestError.ProcessFailed, result);
    }

    [HttpGet]
    [HasPermission("purchaseRequests.view")]
    public async Task<ActionResult<ListPurchaseRequestsWrapperDto>> GetList([FromQuery] PurchaseRequestQueryDto query)
    {
        var result = await _purchaseRequestService.GetListAsync(query);
        if (result.IsSuccess) return Ok(result.Value);

        return this.HandleServerError(PurchaseRequestError.ProcessFailed, result);
    }

    [HttpGet("{id:int}")]
    [HasPermission("purchaseRequests.view")]
    public async Task<ActionResult<PurchaseRequestWrapperDto>> GetById(int id)
    {
        var result = await _purchaseRequestService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return this.HandleServerError(PurchaseRequestError.ProcessFailed, result, id);
    }
}
