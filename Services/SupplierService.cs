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

public class SupplierService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private const int LegacyDefaultTaxConditionId = 1;
    private const decimal LegacyDefaultCreditLimit = 0m;

    public async Task<Result<ListSuppliersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var page = pagination.Page < 1 ? 1 : pagination.Page;
        var pageSize = pagination.PageSize < 1 ? 10 : pagination.PageSize;

        var suppliersQuery = _context.Suppliers.AsNoTracking();
        var totalElements = await suppliersQuery.CountAsync();

        var suppliers = await suppliersQuery
            .OrderBy(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<SupplierResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var paginationData = new Pagination(page, pageSize, totalElements);

        var result = new ListSuppliersWrapperDto
        {
            Suppliers = suppliers,
            Pagination = paginationData
        };

        return Result<ListSuppliersWrapperDto>.Success(result);
    }

    public async Task<Result<SupplierWrapperDto>> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();

        if (supplier == null)
            return Result<SupplierWrapperDto>.Failure(SupplierError.SupplierNotFound, ErrorType.NotFound);

        return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
    }

    public async Task<Result<SupplierWrapperDto>> CreateAsync(CreateSupplierRequestDto request)
    {
        var validationResult = await ValidateCreateOrUpdateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return Result<SupplierWrapperDto>.Failure(validationResult.ErrorMessage!, validationResult.Errors!, ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var entity = new Entity
        {
            EntityTypeId = (int)request.EntityType!.Value,
            DocumentNumber = request.DocumentNumber,
            Phone = request.Phone,
            Address = request.Address,
            Email = request.Email,
            IsActive = request.IsActive
        };

        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();

        await SyncEntitySpecificDataAsync(entity.Id, request);

        var supplier = new Supplier
        {
            EntityId = entity.Id
        };

        _context.Suppliers.Add(supplier);
        _context.Entry(supplier).Property("TaxConditionId").CurrentValue = LegacyDefaultTaxConditionId;
        _context.Entry(supplier).Property("CreditLimit").CurrentValue = LegacyDefaultCreditLimit;
        await _context.SaveChangesAsync();

        await ReplaceSupplierCategoriesAsync(supplier.Id, request.ProductCategoryIds);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
    }

    public async Task<Result<SupplierWrapperDto>> UpdateAsync(int id, UpdateSupplierRequestDto request)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Entity)
                .ThenInclude(e => e.LegalPerson)
            .Include(s => s.Entity)
                .ThenInclude(e => e.PhysicalPerson)
            .Include(s => s.SupplierCategories)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return Result<SupplierWrapperDto>.Failure(SupplierError.SupplierNotFound, ErrorType.NotFound);

        var validationResult = await ValidateCreateOrUpdateRequestAsync(request, supplier.EntityId);
        if (!validationResult.IsSuccess)
            return Result<SupplierWrapperDto>.Failure(validationResult.ErrorMessage!, validationResult.Errors!, ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        supplier.Entity.EntityTypeId = (int)request.EntityType!.Value;
        supplier.Entity.DocumentNumber = request.DocumentNumber;
        supplier.Entity.Phone = request.Phone;
        supplier.Entity.Address = request.Address;
        supplier.Entity.Email = request.Email;
        supplier.Entity.IsActive = request.IsActive;

        await SyncEntitySpecificDataAsync(supplier.EntityId, request);
        await ReplaceSupplierCategoriesAsync(supplier.Id, request.ProductCategoryIds);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
    }

    public async Task<Result<SupplierWrapperDto>> PatchAsync(int id, PatchSupplierRequestDto request)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Entity)
                .ThenInclude(e => e.LegalPerson)
            .Include(s => s.Entity)
                .ThenInclude(e => e.PhysicalPerson)
            .Include(s => s.SupplierCategories)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return Result<SupplierWrapperDto>.Failure(SupplierError.SupplierNotFound, ErrorType.NotFound);

        var targetEntityTypeId = request.EntityType is null
            ? supplier.Entity.EntityTypeId
            : (int)request.EntityType.Value;

        if (!IsSupportedEntityType(targetEntityTypeId))
        {
            return Result<SupplierWrapperDto>.Failure(
                SupplierError.InvalidEntityType,
                new Dictionary<string, string[]> { { "EntityType", [SupplierError.InvalidEntityType] } },
                ErrorType.Validation);
        }

        if (!await _context.EntityTypes.AnyAsync(et => et.Id == targetEntityTypeId))
        {
            return Result<SupplierWrapperDto>.Failure(
                SupplierError.EntityTypeNotConfigured,
                new Dictionary<string, string[]> { { "EntityType", [SupplierError.EntityTypeNotConfigured] } },
                ErrorType.Validation);
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            var exists = await _context.Entities.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber &&
                e.Id != supplier.EntityId);

            if (exists)
            {
                return Result<SupplierWrapperDto>.Failure(
                    SupplierError.DocumentNumberAlreadyExists,
                    new Dictionary<string, string[]> { { "DocumentNumber", [SupplierError.DocumentNumberAlreadyExists] } },
                    ErrorType.Validation);
            }

            supplier.Entity.DocumentNumber = request.DocumentNumber;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        supplier.Entity.EntityTypeId = targetEntityTypeId;
        if (request.Phone is not null)
            supplier.Entity.Phone = request.Phone;
        if (request.Address is not null)
            supplier.Entity.Address = request.Address;
        if (request.Email is not null)
            supplier.Entity.Email = request.Email;
        if (request.IsActive.HasValue)
            supplier.Entity.IsActive = request.IsActive.Value;

        var patchValidationResult = ValidatePatchSpecificData(supplier, request, targetEntityTypeId);
        if (!patchValidationResult.IsSuccess)
            return Result<SupplierWrapperDto>.Failure(patchValidationResult.ErrorMessage!, patchValidationResult.Errors!, ErrorType.Validation);

        await SyncEntitySpecificDataAsync(supplier.EntityId, request, targetEntityTypeId);

        if (request.ProductCategoryIds is not null)
        {
            var categoriesValidation = await ValidateProductCategoriesAsync(request.ProductCategoryIds);
            if (!categoriesValidation.IsSuccess)
                return Result<SupplierWrapperDto>.Failure(categoriesValidation.ErrorMessage!, categoriesValidation.Errors!, ErrorType.Validation);

            await ReplaceSupplierCategoriesAsync(supplier.Id, request.ProductCategoryIds);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<SupplierWrapperDto>.Success(_mapper.Map<SupplierWrapperDto>(supplier));
    }

    private async Task<Result> ValidateCreateOrUpdateRequestAsync(CreateSupplierRequestDto request, int? currentEntityId = null)
    {
        if (request.EntityType is null || !IsSupportedEntityType((int)request.EntityType.Value))
        {
            return Result.Failure(
                SupplierError.InvalidEntityType,
                new Dictionary<string, string[]> { { "EntityType", [SupplierError.InvalidEntityType] } },
                ErrorType.Validation);
        }

        if (!await _context.EntityTypes.AnyAsync(et => et.Id == (int)request.EntityType.Value))
        {
            return Result.Failure(
                SupplierError.EntityTypeNotConfigured,
                new Dictionary<string, string[]> { { "EntityType", [SupplierError.EntityTypeNotConfigured] } },
                ErrorType.Validation);
        }

        var documentExists = await _context.Entities.AnyAsync(e =>
            e.DocumentNumber == request.DocumentNumber &&
            (!currentEntityId.HasValue || e.Id != currentEntityId.Value));

        if (documentExists)
        {
            return Result.Failure(
                SupplierError.DocumentNumberAlreadyExists,
                new Dictionary<string, string[]> { { "DocumentNumber", [SupplierError.DocumentNumberAlreadyExists] } },
                ErrorType.Validation);
        }

        var personValidationResult = ValidatePersonSpecificData(request, (int)request.EntityType.Value);
        if (!personValidationResult.IsSuccess)
            return personValidationResult;

        return await ValidateProductCategoriesAsync(request.ProductCategoryIds);
    }

    private async Task<Result> ValidateCreateOrUpdateRequestAsync(UpdateSupplierRequestDto request, int? currentEntityId = null)
    {
        var createLikeRequest = new CreateSupplierRequestDto
        {
            EntityType = request.EntityType,
            DocumentNumber = request.DocumentNumber,
            Phone = request.Phone,
            Address = request.Address,
            Email = request.Email,
            BusinessName = request.BusinessName,
            FantasyName = request.FantasyName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GenderId = request.GenderId,
            MaritalStatusId = request.MaritalStatusId,
            BirthDate = request.BirthDate,
            IsActive = request.IsActive,
            ProductCategoryIds = request.ProductCategoryIds
        };

        return await ValidateCreateOrUpdateRequestAsync(createLikeRequest, currentEntityId);
    }

    private Result ValidatePersonSpecificData(CreateSupplierRequestDto request, int entityTypeId)
    {
        if (entityTypeId == (int)EntityPersonType.Legal)
        {
            if (string.IsNullOrWhiteSpace(request.BusinessName))
            {
                return Result.Failure(
                    SupplierError.BusinessNameRequired,
                    new Dictionary<string, string[]> { { "BusinessName", [SupplierError.BusinessNameRequired] } },
                    ErrorType.Validation);
            }

            return Result.Success();
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result.Failure(
                SupplierError.FirstNameRequired,
                new Dictionary<string, string[]> { { "FirstName", [SupplierError.FirstNameRequired] } },
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure(
                SupplierError.LastNameRequired,
                new Dictionary<string, string[]> { { "LastName", [SupplierError.LastNameRequired] } },
                ErrorType.Validation);
        }

        if (!request.GenderId.HasValue)
        {
            return Result.Failure(
                SupplierError.GenderRequired,
                new Dictionary<string, string[]> { { "GenderId", [SupplierError.GenderRequired] } },
                ErrorType.Validation);
        }

        if (!request.MaritalStatusId.HasValue)
        {
            return Result.Failure(
                SupplierError.MaritalStatusRequired,
                new Dictionary<string, string[]> { { "MaritalStatusId", [SupplierError.MaritalStatusRequired] } },
                ErrorType.Validation);
        }

        if (!request.BirthDate.HasValue)
        {
            return Result.Failure(
                SupplierError.BirthDateRequired,
                new Dictionary<string, string[]> { { "BirthDate", [SupplierError.BirthDateRequired] } },
                ErrorType.Validation);
        }

        return Result.Success();
    }

    private Result ValidatePatchSpecificData(Supplier supplier, PatchSupplierRequestDto request, int targetEntityTypeId)
    {
        if (targetEntityTypeId == (int)EntityPersonType.Legal)
        {
            var businessName = request.BusinessName ?? supplier.Entity.LegalPerson?.BussinessName;
            if (string.IsNullOrWhiteSpace(businessName))
            {
                return Result.Failure(
                    SupplierError.BusinessNameRequired,
                    new Dictionary<string, string[]> { { "BusinessName", [SupplierError.BusinessNameRequired] } },
                    ErrorType.Validation);
            }

            return Result.Success();
        }

        var firstName = request.FirstName ?? supplier.Entity.PhysicalPerson?.Name;
        var lastName = request.LastName ?? supplier.Entity.PhysicalPerson?.Lastname;
        var genderId = request.GenderId ?? supplier.Entity.PhysicalPerson?.GenderId;
        var maritalStatusId = request.MaritalStatusId ?? supplier.Entity.PhysicalPerson?.MaritalStatusId;
        var birthDate = request.BirthDate ?? supplier.Entity.PhysicalPerson?.BirthDate;

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(
                SupplierError.FirstNameRequired,
                new Dictionary<string, string[]> { { "FirstName", [SupplierError.FirstNameRequired] } },
                ErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure(
                SupplierError.LastNameRequired,
                new Dictionary<string, string[]> { { "LastName", [SupplierError.LastNameRequired] } },
                ErrorType.Validation);
        }

        if (!genderId.HasValue)
        {
            return Result.Failure(
                SupplierError.GenderRequired,
                new Dictionary<string, string[]> { { "GenderId", [SupplierError.GenderRequired] } },
                ErrorType.Validation);
        }

        if (!maritalStatusId.HasValue)
        {
            return Result.Failure(
                SupplierError.MaritalStatusRequired,
                new Dictionary<string, string[]> { { "MaritalStatusId", [SupplierError.MaritalStatusRequired] } },
                ErrorType.Validation);
        }

        if (!birthDate.HasValue)
        {
            return Result.Failure(
                SupplierError.BirthDateRequired,
                new Dictionary<string, string[]> { { "BirthDate", [SupplierError.BirthDateRequired] } },
                ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateProductCategoriesAsync(List<int> productCategoryIds)
    {
        var categoryIds = productCategoryIds.Distinct().ToList();
        if (categoryIds.Count == 0)
            return Result.Success();

        var existingCount = await _context.ProductCategories
            .CountAsync(pc => categoryIds.Contains(pc.Id));

        if (existingCount != categoryIds.Count)
        {
            return Result.Failure(
                SupplierError.InvalidProductCategories,
                new Dictionary<string, string[]> { { "ProductCategoryIds", [SupplierError.InvalidProductCategories] } },
                ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task ReplaceSupplierCategoriesAsync(int supplierId, List<int> productCategoryIds)
    {
        var existing = await _context.SupplierCategories
            .Where(sc => sc.SupplierId == supplierId)
            .ToListAsync();

        _context.SupplierCategories.RemoveRange(existing);

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

    private async Task SyncEntitySpecificDataAsync(int entityId, CreateSupplierRequestDto request)
    {
        await SyncEntitySpecificDataAsync(entityId, request, (int)request.EntityType!.Value);
    }

    private async Task SyncEntitySpecificDataAsync(int entityId, UpdateSupplierRequestDto request)
    {
        await SyncEntitySpecificDataAsync(entityId, request, (int)request.EntityType!.Value);
    }

    private async Task SyncEntitySpecificDataAsync(int entityId, PatchSupplierRequestDto request, int targetEntityTypeId)
    {
        if (targetEntityTypeId == (int)EntityPersonType.Legal)
        {
            var physical = await _context.PhysicalPersons.FirstOrDefaultAsync(pp => pp.EntityId == entityId);
            if (physical is not null)
                _context.PhysicalPersons.Remove(physical);

            var legal = await _context.LegalPersons.FirstOrDefaultAsync(lp => lp.EntityId == entityId);
            if (legal is null)
            {
                legal = new LegalPerson
                {
                    EntityId = entityId,
                    BussinessName = request.BusinessName!,
                    FantasyName = request.FantasyName
                };
                _context.LegalPersons.Add(legal);
            }
            else
            {
                legal.BussinessName = request.BusinessName ?? legal.BussinessName;
                legal.FantasyName = request.FantasyName ?? legal.FantasyName;
            }

            return;
        }

        var legalPerson = await _context.LegalPersons.FirstOrDefaultAsync(lp => lp.EntityId == entityId);
        if (legalPerson is not null)
            _context.LegalPersons.Remove(legalPerson);

        var physicalPerson = await _context.PhysicalPersons.FirstOrDefaultAsync(pp => pp.EntityId == entityId);
        if (physicalPerson is null)
        {
            physicalPerson = new PhysicalPerson
            {
                EntityId = entityId,
                Name = request.FirstName!,
                Lastname = request.LastName!,
                GenderId = request.GenderId!.Value,
                MaritalStatusId = request.MaritalStatusId!.Value,
                BirthDate = request.BirthDate!.Value
            };

            _context.PhysicalPersons.Add(physicalPerson);
        }
        else
        {
            physicalPerson.Name = request.FirstName ?? physicalPerson.Name;
            physicalPerson.Lastname = request.LastName ?? physicalPerson.Lastname;
            physicalPerson.GenderId = request.GenderId ?? physicalPerson.GenderId;
            physicalPerson.MaritalStatusId = request.MaritalStatusId ?? physicalPerson.MaritalStatusId;
            physicalPerson.BirthDate = request.BirthDate ?? physicalPerson.BirthDate;
        }
    }

    private async Task SyncEntitySpecificDataAsync(int entityId, CreateSupplierRequestDto request, int targetEntityTypeId)
    {
        if (targetEntityTypeId == (int)EntityPersonType.Legal)
        {
            var physical = await _context.PhysicalPersons.FirstOrDefaultAsync(pp => pp.EntityId == entityId);
            if (physical is not null)
                _context.PhysicalPersons.Remove(physical);

            var legal = await _context.LegalPersons.FirstOrDefaultAsync(lp => lp.EntityId == entityId);
            if (legal is null)
            {
                legal = new LegalPerson
                {
                    EntityId = entityId,
                    BussinessName = request.BusinessName!,
                    FantasyName = request.FantasyName
                };
                _context.LegalPersons.Add(legal);
            }
            else
            {
                legal.BussinessName = request.BusinessName!;
                legal.FantasyName = request.FantasyName;
            }

            return;
        }

        var legalPerson = await _context.LegalPersons.FirstOrDefaultAsync(lp => lp.EntityId == entityId);
        if (legalPerson is not null)
            _context.LegalPersons.Remove(legalPerson);

        var physicalPerson = await _context.PhysicalPersons.FirstOrDefaultAsync(pp => pp.EntityId == entityId);
        if (physicalPerson is null)
        {
            physicalPerson = new PhysicalPerson
            {
                EntityId = entityId,
                Name = request.FirstName!,
                Lastname = request.LastName!,
                GenderId = request.GenderId!.Value,
                MaritalStatusId = request.MaritalStatusId!.Value,
                BirthDate = request.BirthDate!.Value
            };

            _context.PhysicalPersons.Add(physicalPerson);
        }
        else
        {
            physicalPerson.Name = request.FirstName!;
            physicalPerson.Lastname = request.LastName!;
            physicalPerson.GenderId = request.GenderId!.Value;
            physicalPerson.MaritalStatusId = request.MaritalStatusId!.Value;
            physicalPerson.BirthDate = request.BirthDate!.Value;
        }
    }

    private async Task SyncEntitySpecificDataAsync(int entityId, UpdateSupplierRequestDto request, int targetEntityTypeId)
    {
        var createLikeRequest = new CreateSupplierRequestDto
        {
            EntityType = request.EntityType,
            DocumentNumber = request.DocumentNumber,
            Phone = request.Phone,
            Address = request.Address,
            Email = request.Email,
            BusinessName = request.BusinessName,
            FantasyName = request.FantasyName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GenderId = request.GenderId,
            MaritalStatusId = request.MaritalStatusId,
            BirthDate = request.BirthDate,
            IsActive = request.IsActive,
            ProductCategoryIds = request.ProductCategoryIds
        };

        await SyncEntitySpecificDataAsync(entityId, createLikeRequest, targetEntityTypeId);
    }

    private static bool IsSupportedEntityType(int entityTypeId)
    {
        return entityTypeId is (int)EntityPersonType.Physical or (int)EntityPersonType.Legal;
    }
}
