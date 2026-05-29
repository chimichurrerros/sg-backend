using BackEnd.DTOs.Requests.CustomerQuote;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackEnd.Controllers.CustomerQuote;

[Route("api/customerquotes")]
[ApiController]
[Authorize]
public class CustomerQuoteController(CustomerQuoteService customerQuoteService) : ControllerBase
{
    private readonly CustomerQuoteService _customerQuoteService = customerQuoteService;

    [HttpGet]
    public async Task<ActionResult<ListCustomerQuotesWrapperDto>> GetListCustomerQuotes([FromQuery] CustomerQuoteQueryDto query)
    {
        var result = await _customerQuoteService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListCustomerQuotesWrapperDto>> GetAllCustomerQuotes()
    {
        var result = await _customerQuoteService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerQuoteWrapperDto>> GetCustomerQuoteById(int id)
    {
        var result = await _customerQuoteService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerQuoteWrapperDto>> Create(CreateCustomerQuoteRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var result = await _customerQuoteService.CreateAsync(request, userId);

        if (result.IsSuccess)
            return Created($"/api/customerquotes/{result.Value!.CustomerQuote.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = result.ErrorMessage
            });

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerQuoteWrapperDto>> Update(int id, UpdateCustomerQuoteRequestDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var result = await _customerQuoteService.UpdateAsync(id, request, userId);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        if (result.ErrorType == ErrorType.Conflict)
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = result.ErrorMessage
            });

        return StatusCode(500);
    }
}
