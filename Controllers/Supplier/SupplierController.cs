using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Supplier;

[Route("api/suppliers")]
[ApiController]
[Authorize(Roles = "Admin")]
public class SupplierController(SupplierService supplierService) : ControllerBase
{
    private readonly SupplierService _supplierService = supplierService;

    [HttpGet]
    public async Task<ActionResult<ListSuppliersWrapperDto>> GetListSuppliers([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _supplierService.GetListAsync(pagination);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierWrapperDto>> GetSupplierById(int id)
    {
        var result = await _supplierService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierWrapperDto>> Create(CreateSupplierRequestDto request)
    {
        var result = await _supplierService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/suppliers/{result.Value!.Supplier.Id}", result.Value);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierWrapperDto>> Update(int id, UpdateSupplierRequestDto request)
    {
        var result = await _supplierService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<SupplierWrapperDto>> Patch(int id, PatchSupplierRequestDto request)
    {
        var result = await _supplierService.PatchAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        return StatusCode(500);
    }

}
