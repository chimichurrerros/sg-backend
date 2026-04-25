using BackEnd.DTOs.Requests.BillDetail;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.BillDetail;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.BillDetail;

[Route("api/bill-details")]
[ApiController]
[Authorize]
public class BillDetailController(BillDetailService billDetailService) : ControllerBase
{
    private readonly BillDetailService _billDetailService = billDetailService;

    [HttpGet("by-bill/{billId}")]
    public async Task<ActionResult<ListBillDetailsWrapperDto>> GetListByBillId(int billId, [FromQuery] PaginationRequestDto pagination)
    {
        var result = await _billDetailService.GetListByBillIdAsync(billId, pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillDetailWrapperDto>> GetBillDetailById(int id)
    {
        var result = await _billDetailService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<BillDetailWrapperDto>> Create(CreateBillDetailRequestDto request)
    {
        var result = await _billDetailService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/bill-details/{result.Value!.BillDetail.Id}", result.Value);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillDetailWrapperDto>> Update(int id, UpdateBillDetailRequestDto request)
    {
        var result = await _billDetailService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    // [HttpDelete("{id}")]
    // public async Task<ActionResult> Delete(int id)
    // {
    //     var result = await _billDetailService.DeleteAsync(id);
    //     if (result.IsSuccess) return Ok();
    //     if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
    //     return StatusCode(500);
    // }
}