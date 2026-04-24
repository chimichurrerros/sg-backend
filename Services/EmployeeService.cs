using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Employee;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace BackEnd.Services;

public class EmployeeService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListEmployeesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var employeesQuery = _context.Employees
            .AsNoTracking();

        var totalElements = await employeesQuery.CountAsync();

        var employees = await employeesQuery
            .OrderBy(e => e.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<EmployeeResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var paginationData = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListEmployeesWrapperDto>.Success(new ListEmployeesWrapperDto
        {
            Employees = employees,
            Pagination = paginationData
        });
    }

    public async Task<Result<EmployeeWrapperDto>> GetByIdAsync(int id)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .ProjectTo<EmployeeWrapperDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (employee == null)
            return Result<EmployeeWrapperDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        return Result<EmployeeWrapperDto>.Success(employee);
    }

    public async Task<Result<EmployeeWrapperDto>> CreateAsync(CreateEmployeeRequestDto request)
    {
        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return Result<EmployeeWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var entity = new Entity
            {
                EntityTypeId = (int)EntityPersonType.Physical,
                DocumentNumber = request.DocumentNumber,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email,
                IsActive = request.IsActive
            };

            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();

            var physicalPerson = new PhysicalPerson
            {
                EntityId = entity.Id,
                Name = request.Name,
                Lastname = request.Lastname,
                BirthDate = request.BirthDate,
                GenderId = request.GenderId,
                MaritalStatusId = request.MaritalStatusId
            };

            _context.PhysicalPersons.Add(physicalPerson);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                EntityId = entity.Id,
                FileNumber = request.FileNumber,
                AreaId = request.AreaId,
                InmediatlyBossId = request.InmediatlyBossId,
                HireDate = request.HireDate
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdEmployee = await _context.Employees
                .Where(e => e.Id == employee.Id)
                .ProjectTo<EmployeeWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<EmployeeWrapperDto>.Success(createdEmployee!);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<EmployeeWrapperDto>> UpdateAsync(int id, UpdateEmployeeRequestDto request)
    {
        var employee = await _context.Employees
            .Include(e => e.Entity)
                .ThenInclude(p => p.Entity) // we need the base entity too 
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return Result<EmployeeWrapperDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var validationResult = await ValidateUpdateRequestAsync(request, employee.EntityId);
        if (!validationResult.IsSuccess)
            return Result<EmployeeWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Update Employee properties
            employee.FileNumber = request.FileNumber;
            employee.AreaId = request.AreaId;
            employee.InmediatlyBossId = request.InmediatlyBossId;
            employee.HireDate = request.HireDate;

            // Update Base Entity properties
            employee.Entity.Entity.DocumentNumber = request.DocumentNumber;
            employee.Entity.Entity.Phone = request.Phone;
            employee.Entity.Entity.Address = request.Address;
            employee.Entity.Entity.Email = request.Email;
            employee.Entity.Entity.IsActive = request.IsActive;

            // Update Physical Person properties
            employee.Entity.Name = request.Name;
            employee.Entity.Lastname = request.Lastname;
            employee.Entity.BirthDate = request.BirthDate;
            employee.Entity.GenderId = request.GenderId;
            employee.Entity.MaritalStatusId = request.MaritalStatusId;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updatedEmployee = await _context.Employees
                .Where(e => e.Id == id)
                .ProjectTo<EmployeeWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<EmployeeWrapperDto>.Success(updatedEmployee!);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateEmployeeRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [EmployeeError.DocumentNumberRequired];
        }
        else
        {
            var documentExists = await _context.Entities.AnyAsync(e => e.DocumentNumber == request.DocumentNumber);
            if (documentExists) errors["DocumentNumber"] = [EmployeeError.DocumentNumberAlreadyExists];
        }

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = [EmployeeError.FirstNameRequired];
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = [EmployeeError.LastNameRequired];
        if (string.IsNullOrWhiteSpace(request.FileNumber)) errors["FileNumber"] = [EmployeeError.FileNumberRequired];
        
        var validArea = await _context.Departments.AnyAsync(d => d.Id == request.AreaId);
        if (!validArea) errors["AreaId"] = [EmployeeError.InvalidArea];

        var validGender = await _context.Genders.AnyAsync(g => g.Id == request.GenderId);
        if (!validGender) errors["GenderId"] = [EmployeeError.InvalidGender];

        var validMaritalStatus = await _context.MaritalStatuses.AnyAsync(m => m.Id == request.MaritalStatusId);
        if (!validMaritalStatus) errors["MaritalStatusId"] = [EmployeeError.InvalidMaritalStatus];

        if (request.InmediatlyBossId.HasValue)
        {
            var validBoss = await _context.Employees.AnyAsync(e => e.Id == request.InmediatlyBossId.Value);
            if (!validBoss) errors["InmediatlyBossId"] = [EmployeeError.InvalidInmediatlyBoss];
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdateEmployeeRequestDto request, int currentEntityId)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [EmployeeError.DocumentNumberRequired];
        }
        else
        {
            var documentExists = await _context.Entities.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber && e.Id != currentEntityId);
            if (documentExists) errors["DocumentNumber"] = [EmployeeError.DocumentNumberAlreadyExists];
        }

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = [EmployeeError.FirstNameRequired];
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = [EmployeeError.LastNameRequired];
        if (string.IsNullOrWhiteSpace(request.FileNumber)) errors["FileNumber"] = [EmployeeError.FileNumberRequired];

        var validArea = await _context.Departments.AnyAsync(d => d.Id == request.AreaId);
        if (!validArea) errors["AreaId"] = [EmployeeError.InvalidArea];

        var validGender = await _context.Genders.AnyAsync(g => g.Id == request.GenderId);
        if (!validGender) errors["GenderId"] = [EmployeeError.InvalidGender];

        var validMaritalStatus = await _context.MaritalStatuses.AnyAsync(m => m.Id == request.MaritalStatusId);
        if (!validMaritalStatus) errors["MaritalStatusId"] = [EmployeeError.InvalidMaritalStatus];

        if (request.InmediatlyBossId.HasValue)
        {
            var validBoss = await _context.Employees.AnyAsync(e => e.Id == request.InmediatlyBossId.Value);
            if (!validBoss) errors["InmediatlyBossId"] = [EmployeeError.InvalidInmediatlyBoss];
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }
}
