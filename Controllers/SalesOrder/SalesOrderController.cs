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

namespace BackEnd.Controllers.SalesOrder;

[Route("api/sales-orders")]
[ApiController]
[Authorize]
public class SalesOrderController(SalesOrderService salesOrderService) : ControllerBase
{
    private readonly SalesOrderService _salesOrderService = salesOrderService;

    [HttpPost]
    public async Task<ActionResult<SalesOrderWrapperDto>> Create(CreateSalesOrderRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("User ID claim is missing or invalid.");
        }

        var result = await _salesOrderService.CreateAsync(request, userId);
        if (result.IsSuccess) return Created($"/api/sales-orders/{result.Value!.SalesOrder.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return BadRequest(result.ErrorMessage);

        return Problem(
            title: SalesOrderError.ProcessFailed,
            detail: result.ErrorMessage,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListSalesOrdersWrapperDto>> GetAll()
    {
        var result = await _salesOrderService.GetAllAsync();
        if (result.IsSuccess) return Ok(result.Value);

        return Problem(
            title: "Error al obtener los pedidos de venta",
            detail: result.ErrorMessage,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet()]
    public async Task<ActionResult<ListSalesOrdersWrapperDto>> GetList([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _salesOrderService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);

        return Problem(
            title: "Error al obtener la lista de pedidos de venta",
            detail: result.ErrorMessage,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SalesOrderWrapperDto>> GetById(int id)
    {
        var result = await _salesOrderService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return Problem(
            title: "Error al obtener el pedido de venta",
            detail: result.ErrorMessage,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
