using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.RequestForQuotation;
using BackEnd.DTOs.Responses.RequestForQuotation;
using BackEnd.Constants.Errors;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers;

[Route("api/request-for-quotations")]
[ApiController]
[Authorize]
public class RequestForQuotationController(RequestForQuotationService requestForQuotationService) : ControllerBase
{
    private readonly RequestForQuotationService _requestForQuotationService = requestForQuotationService;

    [HttpGet("all")]
    [HasPermission("requestForQuotations.view")]
    public async Task<ActionResult<ListRequestForQuotationsWrapperDto>> GetAll()
    {
        var result = await _requestForQuotationService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);

        return this.HandleServerError(RequestForQuotationError.ProcessFailed, result);
    }

    [HttpGet]
    [HasPermission("requestForQuotations.view")]
    public async Task<ActionResult<ListRequestForQuotationsWrapperDto>> GetList([FromQuery] RequestForQuotationQueryDto query)
    {
        var result = await _requestForQuotationService.GetListAsync(query);
        if (result.IsSuccess)
            return Ok(result.Value);

        return this.HandleServerError(RequestForQuotationError.ProcessFailed, result);
    }

    [HttpGet("{id:int}")]
    [HasPermission("requestForQuotations.view")]
    public async Task<ActionResult<RequestForQuotationWrapperDto>> GetById(int id)
    {
        var result = await _requestForQuotationService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result, id);

        return this.HandleServerError(RequestForQuotationError.ProcessFailed, result, id);
    }
}
