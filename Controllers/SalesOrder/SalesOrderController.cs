using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        
        if (result.ErrorType == ErrorType.Validation) 
            return BadRequest(result.ErrorMessage);
            
        return StatusCode(500);
    }
}
