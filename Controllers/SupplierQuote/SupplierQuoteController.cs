using BackEnd.DTOs.Requests.SupplierQuote;
using BackEnd.DTOs.Responses.SupplierQuote;
using BackEnd.Services;
using BackEnd.Extensions;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.SupplierQuote;

[Route("api/supplierquotes")]
[ApiController]
[Authorize]
public class SupplierQuoteController(SupplierQuoteService supplierQuoteService) : ControllerBase
{
    private readonly SupplierQuoteService _supplierQuoteService = supplierQuoteService;

    [HttpGet]
    public async Task<ActionResult<ListSupplierQuotesWrapperDto>> GetList([FromQuery] SupplierQuoteQueryDto query)
    {
        var result = await _supplierQuoteService.GetListAsync(query);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ListSupplierQuotesWrapperDto>> GetAll()
    {
        var result = await _supplierQuoteService.GetAllAsync();
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierQuoteWrapperDto>> GetById(int id)
    {
        var result = await _supplierQuoteService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierQuoteWrapperDto>> Create(CreateSupplierQuoteRequestDto request)
    {
        var result = await _supplierQuoteService.CreateAsync(request);
        if (result.IsSuccess)
            return Created($"/api/supplierquotes/{result.Value!.SupplierQuote.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierQuoteWrapperDto>> Update(int id, UpdateSupplierQuoteRequestDto request)
    {
        var result = await _supplierQuoteService.UpdateAsync(id, request);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }
}
