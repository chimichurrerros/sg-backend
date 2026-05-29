using BackEnd.DTOs.Requests.CreditNote;
using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Extensions;

namespace BackEnd.Controllers.CreditNote;

[Route("api/credit-notes")]
[ApiController]
[Authorize]
public class CreditNoteController(CreditNoteService creditNoteService) : ControllerBase
{
    private readonly CreditNoteService _creditNoteService = creditNoteService;

    [HttpPost]
    public async Task<ActionResult<CreditNoteWrapperDto>> Create(CreateCreditNoteDto request)
    {
        var result = await _creditNoteService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/credit-notes/{result.Value!.CreditNote.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CreditNoteWrapperDto>> GetById(int id)
    {
        var result = await _creditNoteService.GetByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
