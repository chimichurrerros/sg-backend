using BackEnd.DTOs.Requests.Service;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Service;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Product;

[Route("api/services")]
[ApiController]
[Authorize]
public class ServiceController(ServicesService ServiceService) : ControllerBase
{
    private readonly ServicesService _serviceService = ServiceService;

    [HttpGet]
    [HasPermission("services.view")]
    public async Task<ActionResult<ListServiceWrapperDto>> GetListProducts([FromQuery] ServiceQueryDto query)
    {
        var result = await _serviceService.GetListAsync(query);

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("services.view")]
    public async Task<ActionResult<ListServiceWrapperDto>> GetAllProducts()
    {
        var result = await _serviceService.GetAllAsync();

        if (result.IsSuccess)
            return Ok(result.Value);

        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("services.view")]
    public async Task<ActionResult<ServiceWrapperDto>> GetProductById(int id)
    {
        var result = await _serviceService.GetByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("services.create")]
    public async Task<ActionResult<ServiceWrapperDto>> Create(ServiceRequestDto request)
    {
        var result = await _serviceService.CreateAsync(request);

        if (result.IsSuccess)
            return Created($"/api/products/{result.Value!.Service.Id}", result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("services.update")]
    public async Task<ActionResult<ServiceWrapperDto>> Update(int id, ServiceRequestDto request)
    {
        var result = await _serviceService.UpdateAsync(id, request);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }

    [HttpDelete("{id}")]
    [HasPermission("services.delete")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _serviceService.DeleteAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        return StatusCode(500);
    }
}
