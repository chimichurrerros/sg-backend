using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

/// <summary>
/// Service responsible for all supplier management business logic.
/// All validation and data manipulation occurs at this layer.
/// Suppliers are always created as LegalPerson entities (not PhysicalPerson).
/// </summary>
public class SupplierService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Retrieves a paginated list of all suppliers.
    /// </summary>
    /// <param name="pagination">Pagination parameters (Page and PageSize)</param>
    /// <returns>Wrapped result containing paginated suppliers and pagination metadata</returns>
    public async Task<Result<ListSuppliersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        // Query suppliers without tracking (read-only operation)
        var suppliersQuery = _context.Suppliers.AsNoTracking();

        // Count total suppliers before applying skip/take for accurate total pages calculation
        var totalElements = await suppliersQuery.CountAsync();

        // Fetch the page of suppliers and project to DTO
        var suppliers = await suppliersQuery
            .OrderBy(s => s.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<SupplierResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        // Create pagination metadata
        var paginationData = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        // Wrap results with pagination info
        var result = new ListSuppliersWrapperDto
        {
            Suppliers = suppliers,
            Pagination = paginationData
        };

        return Result<ListSuppliersWrapperDto>.Success(result);
    }

    /// <summary>
    /// Retrieves a single supplier by ID.
    /// </summary>
    /// <param name="id">The supplier ID</param>
    /// <returns>Wrapped result containing the supplier data or NotFound error</returns>
    public async Task<Result<SupplierWrapperDto>> GetByIdAsync(int id)
    {
        // Query supplier without tracking (read-only operation)
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.Entity)
                .ThenInclude(e => e.LegalPerson)
            .Include(s => s.SupplierCategories)
            .FirstOrDefaultAsync(s => s.Id == id);

        // Return error if supplier not found
        if (supplier == null)
            return Result<SupplierWrapperDto>.Failure(SupplierError.SupplierNotFound, ErrorType.NotFound);

        // Map and wrap the result
        return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
    }

    /// <summary>
    /// Creates a new supplier with automatic Entity and LegalPerson creation.
    /// Process: 1) Validate input, 2) Create Entity (LegalPerson type), 3) Create Supplier, 
    /// 4) Associate Entity to Supplier, 5) Create LegalPerson, 6) Add product categories.
    /// All operations occur within a single database transaction.
    /// </summary>
    /// <param name="request">Create supplier request DTO containing legal entity data</param>
    /// <returns>Wrapped result containing the created supplier with Entity data</returns>
    public async Task<Result<SupplierWrapperDto>> CreateAsync(CreateSupplierRequestDto request)
    {
        // Step 1: Validate all input data
        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return Result<SupplierWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        // Step 2: Begin database transaction to ensure atomicity
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Step 3: Create Entity with LegalPerson type (EntityPersonType.Legal = 2)
            var entity = new Entity
            {
                EntityTypeId = (int)EntityPersonType.Legal, // Always LegalPerson for suppliers
                DocumentNumber = request.DocumentNumber,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email,
                IsActive = request.IsActive
            };

            // Add entity to context and persist
            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();

            // Step 4: Create Supplier and associate to the newly created Entity
            var supplier = new Supplier
            {
                EntityId = entity.Id // Link to the created entity
            };

            // Add supplier to context and persist
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            // Step 5: Create LegalPerson data for the Entity
            var legalPerson = new LegalPerson
            {
                EntityId = entity.Id,
                BussinessName = request.BusinessName,
                FantasyName = request.FantasyName
            };

            // Add legal person data to context and persist
            _context.LegalPersons.Add(legalPerson);
            await _context.SaveChangesAsync();

            // Step 6: Add product categories if provided
            if (request.ProductCategoryIds.Count > 0)
            {
                await AddSupplierCategoriesAsync(supplier.Id, request.ProductCategoryIds);
                await _context.SaveChangesAsync();
            }

            // Commit transaction - all operations succeeded
            await transaction.CommitAsync();

            // Fetch the created supplier with all related data for response
            var createdSupplier = await _context.Suppliers
                .Include(s => s.Entity)
                    .ThenInclude(e => e.LegalPerson)
                .Include(s => s.SupplierCategories)
                .FirstOrDefaultAsync(s => s.Id == supplier.Id);

            // Map to DTO and return success
            return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(createdSupplier));
        }
        catch (Exception)
        {
            // Rollback transaction on any exception
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates an existing supplier completely (replaces all fields).
    /// Process: 1) Load supplier and entity, 2) Validate, 3) Update Entity fields,
    /// 4) Update LegalPerson data, 5) Replace product categories.
    /// </summary>
    /// <param name="id">The supplier ID to update</param>
    /// <param name="request">Update supplier request DTO with new data</param>
    /// <returns>Wrapped result containing the updated supplier or error</returns>
    public async Task<Result<SupplierWrapperDto>> UpdateAsync(int id, UpdateSupplierRequestDto request)
    {
        // Step 1: Load supplier with related Entity and LegalPerson data
        var supplier = await _context.Suppliers
            .Include(s => s.Entity)
                .ThenInclude(e => e.LegalPerson)
            .Include(s => s.SupplierCategories)
            .FirstOrDefaultAsync(s => s.Id == id);

        // Return error if supplier not found
        if (supplier == null)
            return Result<SupplierWrapperDto>.Failure(SupplierError.SupplierNotFound, ErrorType.NotFound);

        // Step 2: Validate all input data
        var validationResult = await ValidateUpdateRequestAsync(request, supplier.EntityId);
        if (!validationResult.IsSuccess)
            return Result<SupplierWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        // Step 3: Begin database transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Step 4: Update Entity fields (document, contact info, etc.)
            supplier.Entity.DocumentNumber = request.DocumentNumber;
            supplier.Entity.Phone = request.Phone;
            supplier.Entity.Address = request.Address;
            supplier.Entity.Email = request.Email;
            supplier.Entity.IsActive = request.IsActive;

            // Step 5: Update or create LegalPerson data
            if (supplier.Entity.LegalPerson == null)
            {
                // Create LegalPerson if it doesn't exist (shouldn't happen, but handle it)
                supplier.Entity.LegalPerson = new LegalPerson
                {
                    EntityId = supplier.Entity.Id,
                    BussinessName = request.BusinessName,
                    FantasyName = request.FantasyName
                };
                _context.LegalPersons.Add(supplier.Entity.LegalPerson);
            }
            else
            {
                // Update existing LegalPerson
                supplier.Entity.LegalPerson.BussinessName = request.BusinessName;
                supplier.Entity.LegalPerson.FantasyName = request.FantasyName;
            }

            // Step 6: Replace product categories with new ones
            await ReplaceSupplierCategoriesAsync(supplier.Id, request.ProductCategoryIds);

            // Persist all changes
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Map to DTO and return success
            return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
        }
        catch (Exception)
        {
            // Rollback transaction on any exception
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Validates the Create request. Checks:
    /// 1) Document number is not empty
    /// 2) Business name is not empty (LegalPerson requirement)
    /// 3) Document number is unique
    /// 4) All product categories exist in database
    /// </summary>
    /// <param name="request">Create supplier request to validate</param>
    /// <returns>Success result or validation error with details</returns>
    private async Task<Result> ValidateCreateRequestAsync(CreateSupplierRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate document number (required and not empty)
        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [SupplierError.DocumentNumberRequired];
        }
        else
        {
            // Check if document already exists (must be unique)
            var documentExists = await _context.Entities.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber);

            if (documentExists)
                errors["DocumentNumber"] = [SupplierError.DocumentNumberAlreadyExists];
        }

        // Validate business name (required for LegalPerson)
        if (string.IsNullOrWhiteSpace(request.BusinessName))
            errors["BusinessName"] = [SupplierError.BusinessNameRequired];

        // If we have basic validation errors, return them immediately
        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        // Validate product categories if provided
        var categoriesValidation = await ValidateProductCategoriesAsync(request.ProductCategoryIds);
        if (!categoriesValidation.IsSuccess)
            return categoriesValidation;

        return Result.Success();
    }

    /// <summary>
    /// Validates the Update request (same validations as Create).
    /// </summary>
    /// <param name="request">Update supplier request to validate</param>
    /// <param name="currentEntityId">The current entity ID (for uniqueness check exclusion)</param>
    /// <returns>Success result or validation error with details</returns>
    private async Task<Result> ValidateUpdateRequestAsync(UpdateSupplierRequestDto request, int currentEntityId)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate document number
        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [SupplierError.DocumentNumberRequired];
        }
        else
        {
            // Check if document already exists (excluding current entity)
            var documentExists = await _context.Entities.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber && e.Id != currentEntityId);

            if (documentExists)
                errors["DocumentNumber"] = [SupplierError.DocumentNumberAlreadyExists];
        }

        // Validate business name
        if (string.IsNullOrWhiteSpace(request.BusinessName))
            errors["BusinessName"] = [SupplierError.BusinessNameRequired];

        // Return errors if validation failed
        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        // Validate product categories
        var categoriesValidation = await ValidateProductCategoriesAsync(request.ProductCategoryIds);
        if (!categoriesValidation.IsSuccess)
            return categoriesValidation;

        return Result.Success();
    }

    /// <summary>
    /// Validates that all provided product category IDs exist in the database.
    /// </summary>
    /// <param name="productCategoryIds">List of category IDs to validate</param>
    /// <returns>Success result or validation error if any category doesn't exist</returns>
    private async Task<Result> ValidateProductCategoriesAsync(List<int> productCategoryIds)
    {
        // If no categories provided, validation passes
        if (productCategoryIds.Count == 0)
            return Result.Success();

        // Get distinct category IDs (remove duplicates)
        var distinctCategoryIds = productCategoryIds.Distinct().ToList();

        // Count how many of the provided categories exist in database
        var existingCount = await _context.ProductCategories
            .CountAsync(pc => distinctCategoryIds.Contains(pc.Id));

        // If counts don't match, some categories don't exist
        if (existingCount != distinctCategoryIds.Count)
        {
            return Result.Failure(
                SupplierError.InvalidProductCategories,
                new Dictionary<string, string[]> { { "ProductCategoryIds", [SupplierError.InvalidProductCategories] } },
                ErrorType.Validation);
        }

        return Result.Success();
    }

    /// <summary>
    /// Replaces all product categories for a supplier with the provided list.
    /// Process: 1) Remove all existing categories, 2) Add new categories from list.
    /// </summary>
    /// <param name="supplierId">The supplier ID</param>
    /// <param name="productCategoryIds">New list of product category IDs</param>
    private async Task ReplaceSupplierCategoriesAsync(int supplierId, List<int> productCategoryIds)
    {
        // Step 1: Remove all existing SupplierCategory associations
        var existingCategories = await _context.SupplierCategories
            .Where(sc => sc.SupplierId == supplierId)
            .ToListAsync();

        _context.SupplierCategories.RemoveRange(existingCategories);

        // Step 2: Add new SupplierCategory associations for distinct category IDs
        var distinctCategoryIds = productCategoryIds.Distinct().ToList();
        foreach (var categoryId in distinctCategoryIds)
        {
            _context.SupplierCategories.Add(new SupplierCategory
            {
                SupplierId = supplierId,
                ProductCategoryId = categoryId
            });
        }
    }

    /// <summary>
    /// Adds product categories for a supplier (without removing existing ones).
    /// </summary>
    /// <param name="supplierId">The supplier ID</param>
    /// <param name="productCategoryIds">List of product category IDs to add</param>
    private async Task AddSupplierCategoriesAsync(int supplierId, List<int> productCategoryIds)
    {
        // Get distinct category IDs to avoid duplicates
        var distinctCategoryIds = productCategoryIds.Distinct().ToList();

        // Add new SupplierCategory associations
        foreach (var categoryId in distinctCategoryIds)
        {
            _context.SupplierCategories.Add(new SupplierCategory
            {
                SupplierId = supplierId,
                ProductCategoryId = categoryId
            });
        }
    }
}
