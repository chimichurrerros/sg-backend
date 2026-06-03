using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.Constants.Errors;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BackEnd.DTOs.Requests.Pagination;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.SalesOrder;

[Route("api/sales-orders")]
[ApiController]
[Authorize]
public class SalesOrderController(SalesOrderService salesOrderService) : ControllerBase
{
    private readonly SalesOrderService _salesOrderService = salesOrderService;

    [HttpPost]
    [HasPermission("salesOrders.create")]
    public async Task<ActionResult<SalesOrderWrapperDto>> Create(CreateSalesOrderRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var result = await _salesOrderService.CreateAsync(request, userId);
        if (result.IsSuccess) return Created($"/api/sales-orders/{result.Value!.SalesOrder.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, null);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return this.HandleServerError(SalesOrderError.ProcessFailed, result);
    }

    [HttpPost("pos")]
    [HasPermission("salesOrders.create")]
    public async Task<ActionResult<SalesOrderWrapperDto>> CreateFromPos(CreatePosSaleRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var result = await _salesOrderService.CreateFromPosAsync(request, userId);
        if (result.IsSuccess) return Created($"/api/sales-orders/{result.Value!.SalesOrder.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, null);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return this.HandleServerError(SalesOrderError.ProcessFailed, result);
    }

    [HttpGet("all")]
    [HasPermission("salesOrders.view")]
    public async Task<ActionResult<ListSalesOrdersWrapperDto>> GetAll()
    {
        var result = await _salesOrderService.GetAllAsync();
        if (result.IsSuccess) return Ok(result.Value);

        return this.HandleServerError(SalesOrderError.ProcessFailed, result);
    }

    [HttpGet()]
    [HasPermission("salesOrders.view")]
    public async Task<ActionResult<ListSalesOrdersWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _salesOrderService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);

        return this.HandleServerError(SalesOrderError.ProcessFailed, result);
    }

    [HttpGet("{id:int}")]
    [HasPermission("salesOrders.view")]
    public async Task<ActionResult<SalesOrderWrapperDto>> GetById(int id)
    {
        var result = await _salesOrderService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return this.HandleServerError(SalesOrderError.ProcessFailed, result, id);
    }
}
