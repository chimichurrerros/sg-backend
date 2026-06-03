using BackEnd.DTOs.Requests.Customer;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Customer;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Customer;

[Route("api/customers")]
[ApiController]
[Authorize]
public class CustomerController(CustomerService customerService) : ControllerBase
{
    private readonly CustomerService _customerService = customerService;

    [HttpGet]
    [HasPermission("customers.view")]
    public async Task<ActionResult<ListCustomersWrapperDto>> GetListCustomers([FromQuery] PaginationRequestDto pagination)
    {
        var result = await _customerService.GetListAsync(pagination);
        if (result.IsSuccess) return Ok(result.Value);
        return StatusCode(500);
    }

    [HttpGet("{id}")]
    [HasPermission("customers.view")]
    public async Task<ActionResult<CustomerWrapperDto>> GetCustomerById(int id)
    {
        var result = await _customerService.GetByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        return StatusCode(500);
    }

    [HttpPost]
    [HasPermission("customers.create")]
    public async Task<ActionResult<CustomerWrapperDto>> Create(CreateCustomerRequestDto request)
    {
        var result = await _customerService.CreateAsync(request);
        if (result.IsSuccess) return Created($"/api/customers/{result.Value!.Customer.Id}", result.Value);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpPut("{id}")]
    [HasPermission("customers.update")]
    public async Task<ActionResult<CustomerWrapperDto>> Update(int id, UpdateCustomerRequestDto request)
    {
        var result = await _customerService.UpdateAsync(id, request);
        if (result.IsSuccess) return Ok(result.Value);
        if (result.ErrorType == ErrorType.NotFound) return this.HandleNotFoundProblem(result);
        if (result.ErrorType == ErrorType.Validation) return this.HandleValidationProblem(result);
        return StatusCode(500);
    }

    [HttpGet("all")]
    [HasPermission("customers.view")]
    public async Task<ActionResult<ListCustomersWrapperDto>> GetAll()
    {
        var result = await _customerService.GetAllAsync();
        if (result.IsSuccess) return Ok(result.Value);
        else return this.HandleBadRequestProblem(result);

    }
}
