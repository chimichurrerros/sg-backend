using BackEnd.DTOs.Requests.CustomerQuote;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.CustomerQuote;

/// <summary>
/// Controller for managing customer quote operations.
/// This controller handles HTTP input/output and delegates all business logic to CustomerQuoteService.
/// </summary>
[Route("api/customerquotes")]
[ApiController]
[Authorize]
public class CustomerQuoteController(CustomerQuoteService customerQuoteService) : ControllerBase
{
    private readonly CustomerQuoteService _customerQuoteService = customerQuoteService;

    /// <summary>
    /// Retrieves a paginated list of customer quotes.
    /// </summary>
    /// <param name="pagination">Pagination parameters from query string.</param>
    /// <returns>Paginated list of quotes.</returns>
    [HttpGet]
    public async Task<ActionResult<ListCustomerQuotesWrapperDto>> GetListCustomerQuotes([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _customerQuoteService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    /// <summary>
    /// Retrieves all customer quotes without pagination.
    /// </summary>
    /// <returns>Complete list of quotes.</returns>
    [HttpGet("all")]
    public async Task<ActionResult<ListCustomerQuotesWrapperDto>> GetAllCustomerQuotes()
    {
        var result = await _customerQuoteService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    /// <summary>
    /// Retrieves one customer quote by its identifier.
    /// </summary>
    /// <param name="id">Quote identifier.</param>
    /// <returns>Single quote payload.</returns>
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

    /// <summary>
    /// Creates a new customer quote.
    /// </summary>
    /// <param name="request">Quote header and detail lines.</param>
    /// <returns>Created quote payload.</returns>
    [HttpPost]
    public async Task<ActionResult<CustomerQuoteWrapperDto>> Create(CreateCustomerQuoteRequestDto request)
    {
        var result = await _customerQuoteService.CreateAsync(request);

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

    /// <summary>
    /// Updates an existing customer quote.
    /// </summary>
    /// <param name="id">Quote identifier.</param>
    /// <param name="request">Updated quote payload.</param>
    /// <returns>Updated quote payload.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerQuoteWrapperDto>> Update(int id, UpdateCustomerQuoteRequestDto request)
    {
        var result = await _customerQuoteService.UpdateAsync(id, request);

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
