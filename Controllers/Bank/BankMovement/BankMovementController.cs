using BackEnd.DTOs.Requests.Bank.BankMovement;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Bank.BankMovement;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Bank.BankMovement;

[Route("api/bank-movements")]
[ApiController]
[Authorize]
public class BankMovementsController(BankMovementService bankMovementService) : ControllerBase
{
    private readonly BankMovementService _bankMovementService = bankMovementService;

    [HttpGet]
    [HasPermission("bankMovements.view")]
    public async Task<ActionResult<ListBankMovementsWrapperDto>> GetListBankMovements([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _bankMovementService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("bankMovements.view")]
    public async Task<ActionResult<ListBankMovementsWrapperDto>> GetAllBankMovements()
    {
        var result = await _bankMovementService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("bankMovements.view")]
    public async Task<ActionResult<BankMovementWrapperDto>> GetById(int id)
    {
        var result = await _bankMovementService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("bankMovements.create")]
    public async Task<ActionResult<BankMovementWrapperDto>> Create(BankMovementRequestDto request)
    {
        var result = await _bankMovementService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/bank-movements/{result.Value!.BankMovement.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        // Si falla por la validación de negocio (saldo insuficiente), devuelve BadRequest
        if (result.ErrorType == ErrorType.Validation)
            return BadRequest(result);

        return StatusCode(500);
    }
}
