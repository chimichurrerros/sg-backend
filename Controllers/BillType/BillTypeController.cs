using BackEnd.DTOs.Requests.BillType;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.BillType;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.BillType;

[Route("api/bill-types")]
[ApiController]
[Authorize]
public class BillTypeController(BillTypeService billTypeService) : ControllerBase
{
    private readonly BillTypeService _billTypeService = billTypeService;

    [HttpGet]
    public async Task<ActionResult<ListBillTypesWrapperDto>> GetListBillTypes([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _billTypeService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillTypeWrapperDto>> GetBillTypeById(int id)
    {
        var result = await _billTypeService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<BillTypeWrapperDto>> Create(CreateBillTypeRequestDto request)
    {
        var result = await _billTypeService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/bill-types/{result.Value!.BillType.Id}", result.Value);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BillTypeWrapperDto>> Update(int id, UpdateBillTypeRequestDto request)
    {
        var result = await _billTypeService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    // [HttpDelete("{id}")]
    // public async Task<ActionResult> Delete(int id)
    // {
    //     var result = await _billTypeService.DeleteAsync(id);
    //     if (result.IsSuccess) return Ok();
    //     if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
    //     return StatusCode(500);
    // }
}