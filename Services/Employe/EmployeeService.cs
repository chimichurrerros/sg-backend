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
                GenderId = request.GenderId
            };

            _context.PhysicalPersons.Add(physicalPerson);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                EntityId = entity.Id,
                FileNumber = request.FileNumber,
                AreaId = request.AreaId,
                BranchId = request.BranchId,
                InmediatlyBossId = request.InmediatlyBossId,
                HireDate = request.HireDate,
                MaritalStatus = request.MaritalStatus
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var initialPositionHistory = new PositionByScheduleByEmployee
            {
                EmployeeId = employee.Id,
                PositionId = request.PositionId,
                ScheduleId = request.ScheduleId,
                BasicSalary = request.BasicSalary,
                StartDate = request.PositionStartDate
            };

            _context.PositionByScheduleByEmployees.Add(initialPositionHistory);
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
            employee.BranchId = request.BranchId;
            employee.InmediatlyBossId = request.InmediatlyBossId;
            employee.HireDate = request.HireDate;
            employee.MaritalStatus = request.MaritalStatus;

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

    public async Task<Result<ListEmployeePositionHistoriesWrapperDto>> GetPositionHistoriesAsync(int employeeId)
    {
        var employeeExists = await _context.Employees.AsNoTracking().AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            return Result<ListEmployeePositionHistoriesWrapperDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var histories = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(history => history.EmployeeId == employeeId)
            .OrderByDescending(history => history.StartDate)
            .ThenByDescending(history => history.Id)
            .Select(history => new EmployeePositionHistoryResponseDto
            {
                Id = history.Id,
                EmployeeId = history.EmployeeId,
                PositionId = history.PositionId,
                PositionName = history.Position.Name,
                ScheduleId = history.ScheduleId,
                ScheduleName = history.Schedule.ScheduleType.Name,
                BasicSalary = history.BasicSalary,
                StartDate = history.StartDate,
                EndDate = history.EndDate
            })
            .ToListAsync();

        return Result<ListEmployeePositionHistoriesWrapperDto>.Success(new ListEmployeePositionHistoriesWrapperDto
        {
            Histories = histories
        });
    }

    public async Task<Result<EmployeePositionHistoryWrapperDto>> AddPositionHistoryAsync(int employeeId, CreateEmployeePositionHistoryRequestDto request)
    {
        var validation = await ValidatePositionHistoryRequestAsync(employeeId, request);
        if (!validation.IsSuccess)
            return Result<EmployeePositionHistoryWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var currentOpenHistory = await _context.PositionByScheduleByEmployees
                .Where(history => history.EmployeeId == employeeId && history.EndDate == null)
                .OrderByDescending(history => history.StartDate)
                .ThenByDescending(history => history.Id)
                .FirstOrDefaultAsync();

            if (currentOpenHistory != null)
            {
                currentOpenHistory.EndDate = request.StartDate.AddDays(-1);
            }

            var newHistory = new PositionByScheduleByEmployee
            {
                EmployeeId = employeeId,
                PositionId = request.PositionId,
                ScheduleId = request.ScheduleId,
                BasicSalary = request.BasicSalary,
                StartDate = request.StartDate,
                EndDate = null
            };

            _context.PositionByScheduleByEmployees.Add(newHistory);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdHistory = await _context.PositionByScheduleByEmployees
                .AsNoTracking()
                .Where(history => history.Id == newHistory.Id)
                .Select(history => new EmployeePositionHistoryResponseDto
                {
                    Id = history.Id,
                    EmployeeId = history.EmployeeId,
                    PositionId = history.PositionId,
                    PositionName = history.Position.Name,
                    ScheduleId = history.ScheduleId,
                    ScheduleName = history.Schedule.ScheduleType.Name,
                    BasicSalary = history.BasicSalary,
                    StartDate = history.StartDate,
                    EndDate = history.EndDate
                })
                .FirstOrDefaultAsync();

            return Result<EmployeePositionHistoryWrapperDto>.Success(new EmployeePositionHistoryWrapperDto
            {
                History = createdHistory!
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<ListEmployeeRelationsWrapperDto>> GetRelationsAsync(int employeeId)
    {
        var employeeExists = await _context.Employees.AsNoTracking().AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            return Result<ListEmployeeRelationsWrapperDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var relations = await _context.EmployeeRelations
            .AsNoTracking()
            .Where(relation => relation.EmployeeId == employeeId)
            .OrderBy(relation => relation.RelationType)
            .ThenBy(relation => relation.Name)
            .ThenBy(relation => relation.Lastname)
            .Select(relation => new EmployeeRelationResponseDto
            {
                Id = relation.Id,
                EmployeeId = relation.EmployeeId,
                RelationType = relation.RelationType,
                Name = relation.Name,
                Lastname = relation.Lastname,
                DocumentNumber = relation.DocumentNumber,
                BirthDate = relation.BirthDate,
                StartDate = relation.StartDate,
                EndDate = relation.EndDate
            })
            .ToListAsync();

        return Result<ListEmployeeRelationsWrapperDto>.Success(new ListEmployeeRelationsWrapperDto
        {
            Relations = relations
        });
    }

    public async Task<Result<EmployeeRelationWrapperDto>> AddRelationAsync(int employeeId, CreateEmployeeRelationRequestDto request)
    {
        var validation = await ValidateRelationRequestAsync(employeeId, request);
        if (!validation.IsSuccess)
            return Result<EmployeeRelationWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, ErrorType.Validation);

        var relation = new EmployeeRelation
        {
            EmployeeId = employeeId,
            RelationType = request.RelationType,
            Name = request.Name.Trim(),
            Lastname = request.Lastname.Trim(),
            DocumentNumber = request.DocumentNumber.Trim(),
            BirthDate = request.BirthDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.EmployeeRelations.Add(relation);
        await _context.SaveChangesAsync();

        return Result<EmployeeRelationWrapperDto>.Success(new EmployeeRelationWrapperDto
        {
            Relation = new EmployeeRelationResponseDto
            {
                Id = relation.Id,
                EmployeeId = relation.EmployeeId,
                RelationType = relation.RelationType,
                Name = relation.Name,
                Lastname = relation.Lastname,
                DocumentNumber = relation.DocumentNumber,
                BirthDate = relation.BirthDate,
                StartDate = relation.StartDate,
                EndDate = relation.EndDate
            }
        });
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

        if (request.BranchId.HasValue)
        {
            var validBranch = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId.Value);
            if (!validBranch) errors["BranchId"] = [EmployeeError.InvalidBranch];
        }

        var validPosition = await _context.Positions.AnyAsync(position => position.Id == request.PositionId);
        if (!validPosition) errors["PositionId"] = [EmployeeError.InvalidPosition];

        var validSchedule = await _context.Schedules.AnyAsync(schedule => schedule.Id == request.ScheduleId);
        if (!validSchedule) errors["ScheduleId"] = [EmployeeError.InvalidSchedule];

        if (request.BasicSalary <= 0) errors["BasicSalary"] = [EmployeeError.BasicSalaryMustBeGreaterThanZero];
        if (request.PositionStartDate < request.HireDate) errors["PositionStartDate"] = [EmployeeError.PositionStartDateBeforeHireDate];

        var validGender = await _context.Genders.AnyAsync(g => g.Id == request.GenderId);
        if (!validGender) errors["GenderId"] = [EmployeeError.InvalidGender];

        if (!Enum.IsDefined(request.MaritalStatus)) errors["MaritalStatus"] = [EmployeeError.InvalidMaritalStatus];

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

        if (request.BranchId.HasValue)
        {
            var validBranch = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId.Value);
            if (!validBranch) errors["BranchId"] = [EmployeeError.InvalidBranch];
        }

        var validGender = await _context.Genders.AnyAsync(g => g.Id == request.GenderId);
        if (!validGender) errors["GenderId"] = [EmployeeError.InvalidGender];

        if (!Enum.IsDefined(request.MaritalStatus)) errors["MaritalStatus"] = [EmployeeError.InvalidMaritalStatus];

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

    private async Task<Result> ValidatePositionHistoryRequestAsync(int employeeId, CreateEmployeePositionHistoryRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        var employee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => new { e.Id, e.HireDate })
            .FirstOrDefaultAsync();

        if (employee == null)
            return Result.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var validPosition = await _context.Positions.AnyAsync(position => position.Id == request.PositionId);
        if (!validPosition) errors["PositionId"] = [EmployeeError.InvalidPosition];

        var validSchedule = await _context.Schedules.AnyAsync(schedule => schedule.Id == request.ScheduleId);
        if (!validSchedule) errors["ScheduleId"] = [EmployeeError.InvalidSchedule];

        if (request.BasicSalary <= 0) errors["BasicSalary"] = [EmployeeError.BasicSalaryMustBeGreaterThanZero];

        if (request.StartDate < employee.HireDate) errors["StartDate"] = [EmployeeError.PositionStartDateBeforeHireDate];

        var currentOpenHistory = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(history => history.EmployeeId == employeeId && history.EndDate == null)
            .OrderByDescending(history => history.StartDate)
            .ThenByDescending(history => history.Id)
            .FirstOrDefaultAsync();

        if (currentOpenHistory != null && request.StartDate <= currentOpenHistory.StartDate)
            errors["StartDate"] = [EmployeeError.PositionStartDateMustBeAfterCurrent];

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateRelationRequestAsync(int employeeId, CreateEmployeeRelationRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        var employeeExists = await _context.Employees.AsNoTracking().AnyAsync(e => e.Id == employeeId);
        if (!employeeExists)
            return Result.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        if (!Enum.IsDefined(request.RelationType))
            errors["RelationType"] = [EmployeeError.FamilyRelationTypeInvalid];

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = [EmployeeError.FamilyNameRequired];
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = [EmployeeError.FamilyLastnameRequired];
        if (string.IsNullOrWhiteSpace(request.DocumentNumber)) errors["DocumentNumber"] = [EmployeeError.FamilyDocumentRequired];

        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            errors["EndDate"] = [EmployeeError.FamilyEndDateInvalid];

        var duplicatedDocument = await _context.EmployeeRelations.AsNoTracking().AnyAsync(relation =>
            relation.EmployeeId == employeeId && relation.DocumentNumber == request.DocumentNumber.Trim());

        if (duplicatedDocument) errors["DocumentNumber"] = [EmployeeError.FamilyDocumentAlreadyExists];

        if (request.RelationType == EmployeeRelation.RelationTypeEnum.Spouse)
        {
            var hasActiveSpouse = await _context.EmployeeRelations
                .AsNoTracking()
                .AnyAsync(relation =>
                    relation.EmployeeId == employeeId
                    && relation.RelationType == EmployeeRelation.RelationTypeEnum.Spouse
                    && relation.EndDate == null);

            var newSpouseIsActive = request.EndDate == null;
            if (hasActiveSpouse && newSpouseIsActive)
                errors["RelationType"] = [EmployeeError.EmployeeCanOnlyHaveOneActiveSpouse];
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }
}
