using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Supplier;

/// <summary>
/// Controller for managing supplier operations.
/// This controller is responsible ONLY for:
/// 1) Receiving and validating HTTP requests
/// 2) Delegating all business logic to SupplierService
/// 3) Returning appropriate HTTP responses
/// 
/// All complex validation and data manipulation logic resides in SupplierService.
/// </summary>
[Route("api/suppliers")]
[ApiController]
[Authorize]
public class SupplierController(SupplierService supplierService) : ControllerBase
{
    private readonly SupplierService _supplierService = supplierService;

    /// <summary>
    /// GET /api/suppliers
    /// Retrieves a paginated list of all suppliers.
    /// Authentication: Required (Bearer token or cookie)
    /// </summary>
    /// <param name="pagination">Pagination parameters from query string (Page, PageSize)</param>
    /// <returns>Paginated list of suppliers with metadata</returns>
    [HttpGet]
    public async Task<ActionResult<ListSuppliersWrapperDto>> GetListSuppliers([FromQuery] PaginationRequestDto pagination)
    {
        // Delegate to service to retrieve paginated suppliers
        var result = await _supplierService.GetListAsync(pagination);

        // Return successful result
        if (result.IsSuccess)
            return Ok(result.Value);

        // Return server error if operation failed
        return StatusCode(500);
    }

    /// <summary>
    /// GET /api/suppliers/{id}
    /// Retrieves a single supplier by ID.
    /// Authentication: Required (Bearer token or cookie)
    /// </summary>
    /// <param name="id">The supplier ID</param>
    /// <returns>Supplier data with associated entity and categories</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierWrapperDto>> GetSupplierById(int id)
    {
        // Delegate to service to retrieve supplier by ID
        var result = await _supplierService.GetByIdAsync(id);

        // Return successful result
        if (result.IsSuccess)
            return Ok(result.Value);

        // Return 404 if supplier not found
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        // Return server error if operation failed
        return StatusCode(500);
    }

    /// <summary>
    /// POST /api/suppliers
    /// Creates a new supplier with automatic Entity and LegalPerson creation.
    /// The request is validated by data annotations (required fields, email format).
    /// All business logic validation is handled in SupplierService.
    /// Authentication: Required (Bearer token or cookie)
    /// </summary>
    /// <param name="request">Create supplier request containing legal entity data</param>
    /// <returns>Created supplier with 201 status and Location header</returns>
    [HttpPost]
    public async Task<ActionResult<SupplierWrapperDto>> Create(CreateSupplierRequestDto request)
    {
        // Delegate all business logic to service (Entity creation, LegalPerson sync, categories, etc.)
        var result = await _supplierService.CreateAsync(request);

        // Return created result with Location header (201 Created)
        if (result.IsSuccess)
            return Created($"/api/suppliers/{result.Value!.Supplier.Id}", result.Value);

        // Return validation error if business logic validation failed
        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        // Return server error if operation failed
        return StatusCode(500);
    }

    /// <summary>
    /// PUT /api/suppliers/{id}
    /// Updates an existing supplier completely (full replacement).
    /// All fields in the request are used to replace the existing supplier.
    /// Authentication: Required (Bearer token or cookie)
    /// </summary>
    /// <param name="id">The supplier ID to update</param>
    /// <param name="request">Update supplier request with all required fields</param>
    /// <returns>Updated supplier data</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierWrapperDto>> Update(int id, UpdateSupplierRequestDto request)
    {
        // Delegate all business logic to service (Entity/LegalPerson update, categories, etc.)
        var result = await _supplierService.UpdateAsync(id, request);

        // Return successful result (200 OK)
        if (result.IsSuccess)
            return Ok(result.Value);

        // Return 404 if supplier not found
        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        // Return validation error if business logic validation failed
        if (result.ErrorType == ErrorType.Validation)
            return this.HandleValidationProblem(result);

        // Return server error if operation failed
        return StatusCode(500);
    }
}
