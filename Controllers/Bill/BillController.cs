using BackEnd.DTOs.Requests.Bill;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Bill;

[Route("api/bills")]
[ApiController]
[Authorize]
public class BillController(BillService billService) : ControllerBase
{
    private readonly BillService _billService = billService;

    [HttpGet]
    public async Task<ActionResult<ListBillsWrapperDto>> GetListBills([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _billService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillWrapperDto>> GetBillById(int id)
    {
        var result = await _billService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<BillWrapperDto>> Create(CreateBillRequestDto request)
    {
        var result = await _billService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/bills/{result.Value!.Bill.Id}", result.Value);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillWrapperDto>> Update(int id, UpdateBillRequestDto request)
    {
        var result = await _billService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    // [HttpDelete("{id}")]
    // public async Task<ActionResult> Delete(int id)
    // {
    //     var result = await _billService.DeleteAsync(id);
    //     if (result.IsSuccess) return Ok();
    //     if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
    //     return StatusCode(500);
    // }
}