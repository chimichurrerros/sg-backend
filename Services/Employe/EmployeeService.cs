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
            if (string.IsNullOrWhiteSpace(request.FileNumber))
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext('employee_file_number_gen'))");

                var yearSuffix = (DateTime.UtcNow.Year % 1000).ToString("D3");
                var prefix = $"PY{yearSuffix}";

                var maxFileNumber = await _context.Employees
                    .Where(e => e.FileNumber.StartsWith(prefix))
                    .OrderByDescending(e => e.FileNumber)
                    .Select(e => e.FileNumber)
                    .FirstOrDefaultAsync();

                var nextSeq = 1;
                if (maxFileNumber != null)
                {
                    var numericPart = maxFileNumber[prefix.Length..];
                    if (int.TryParse(numericPart, out var lastSeq))
                    {
                        nextSeq = lastSeq + 1;
                    }
                }

                request.FileNumber = $"{prefix}{nextSeq}";
            }

            var employee = new Employee
            {
                FileNumber = request.FileNumber!,
                Name = request.Name,
                Lastname = request.Lastname,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                DocumentNumber = request.DocumentNumber,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email,
                IsActive = request.IsActive,
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
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return Result<EmployeeWrapperDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var validationResult = await ValidateUpdateRequestAsync(request, employee.Id);
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
            employee.Gender = request.Gender;
            employee.HireDate = request.HireDate;
            employee.MaritalStatus = request.MaritalStatus;
            employee.Name = request.Name;
            employee.Lastname = request.Lastname;
            employee.BirthDate = request.BirthDate;
            employee.Gender = request.Gender;
            employee.DocumentNumber = request.DocumentNumber;
            employee.Phone = request.Phone;
            employee.Address = request.Address;
            employee.Email = request.Email;
            employee.IsActive = request.IsActive;

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

    public async Task<Result> DeleteAsync(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return Result.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        if (!employee.IsActive)
            return Result.Failure(EmployeeError.EmployeeAlreadyInactive, ErrorType.Conflict);

        employee.IsActive = false;
        await _context.SaveChangesAsync();

        return Result.Success();
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
                ScheduleType = history.Schedule.ScheduleType,
                ScheduleName = history.Schedule.ScheduleType == ScheduleTypeEnum.Morning ? "Turno Mañana" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.Afternoon ? "Turno Tarde" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.Night ? "Turno Noche" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.FullTime ? "Jornada Completa" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.PartTime ? "Medio Tiempo" :
                    "Desconocido",
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
                    ScheduleType = history.Schedule.ScheduleType,
                    ScheduleName = history.Schedule.ScheduleType == ScheduleTypeEnum.Morning ? "Turno Mañana" :
                        history.Schedule.ScheduleType == ScheduleTypeEnum.Afternoon ? "Turno Tarde" :
                        history.Schedule.ScheduleType == ScheduleTypeEnum.Night ? "Turno Noche" :
                        history.Schedule.ScheduleType == ScheduleTypeEnum.FullTime ? "Jornada Completa" :
                        history.Schedule.ScheduleType == ScheduleTypeEnum.PartTime ? "Medio Tiempo" :
                        "Desconocido",
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

    public async Task<Result<EmployeePositionHistoryWrapperDto>> UpdatePositionHistoryAsync(int employeeId, int historyId, UpdateEmployeePositionHistoryRequestDto request)
    {
        var validation = await ValidateUpdatePositionHistoryRequestAsync(employeeId, historyId, request);
        if (!validation.IsSuccess)
            return Result<EmployeePositionHistoryWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, ErrorType.Validation);

        var history = await _context.PositionByScheduleByEmployees.FirstAsync(h => h.Id == historyId);
        history.PositionId = request.PositionId;
        history.ScheduleId = request.ScheduleId;
        history.BasicSalary = request.BasicSalary;
        history.StartDate = request.StartDate;
        history.EndDate = request.EndDate;

        await _context.SaveChangesAsync();

        var updated = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(h => h.Id == historyId)
            .Select(history => new EmployeePositionHistoryResponseDto
            {
                Id = history.Id,
                EmployeeId = history.EmployeeId,
                PositionId = history.PositionId,
                PositionName = history.Position.Name,
                ScheduleId = history.ScheduleId,
                ScheduleType = history.Schedule.ScheduleType,
                ScheduleName = history.Schedule.ScheduleType == ScheduleTypeEnum.Morning ? "Turno Mañana" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.Afternoon ? "Turno Tarde" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.Night ? "Turno Noche" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.FullTime ? "Jornada Completa" :
                    history.Schedule.ScheduleType == ScheduleTypeEnum.PartTime ? "Medio Tiempo" :
                    "Desconocido",
                BasicSalary = history.BasicSalary,
                StartDate = history.StartDate,
                EndDate = history.EndDate
            })
            .FirstAsync();

        return Result<EmployeePositionHistoryWrapperDto>.Success(new EmployeePositionHistoryWrapperDto { History = updated });
    }

    public async Task<Result> DeletePositionHistoryAsync(int employeeId, int historyId)
    {
        var history = await _context.PositionByScheduleByEmployees
            .FirstOrDefaultAsync(h => h.Id == historyId && h.EmployeeId == employeeId);

        if (history == null)
            return Result.Failure(EmployeeError.PositionHistoryNotFound, ErrorType.NotFound);

        var latestHistory = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(h => h.EmployeeId == employeeId)
            .OrderByDescending(h => h.StartDate)
            .ThenByDescending(h => h.Id)
            .Select(h => new { h.Id })
            .FirstAsync();

        if (latestHistory.Id != historyId)
            return Result.Failure(EmployeeError.PositionHistoryNotDeletable, ErrorType.Conflict);

        _context.PositionByScheduleByEmployees.Remove(history);
        await _context.SaveChangesAsync();

        return Result.Success();
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
                BirthDate = relation.BirthDate
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
            BirthDate = request.BirthDate
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
                BirthDate = relation.BirthDate
            }
        });
    }

    public async Task<Result<EmployeeRelationWrapperDto>> UpdateRelationAsync(int employeeId, int relationId, UpdateEmployeeRelationRequestDto request)
    {
        var validation = await ValidateUpdateRelationRequestAsync(employeeId, relationId, request);
        if (!validation.IsSuccess)
            return Result<EmployeeRelationWrapperDto>.Failure(validation.ErrorMessage!, validation.Errors!, ErrorType.Validation);

        var relation = await _context.EmployeeRelations.FirstAsync(relation => relation.Id == relationId);
        relation.RelationType = request.RelationType;
        relation.Name = request.Name.Trim();
        relation.Lastname = request.Lastname.Trim();
        relation.DocumentNumber = request.DocumentNumber.Trim();
        relation.BirthDate = request.BirthDate;

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
                BirthDate = relation.BirthDate
            }
        });
    }

    public async Task<Result> DeleteRelationAsync(int employeeId, int relationId)
    {
        var relation = await _context.EmployeeRelations
            .FirstOrDefaultAsync(relation => relation.Id == relationId && relation.EmployeeId == employeeId);

        if (relation == null)
            return Result.Failure(EmployeeError.FamilyRelationNotFound, ErrorType.NotFound);

        _context.EmployeeRelations.Remove(relation);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateEmployeeRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = new[] { EmployeeError.DocumentNumberRequired };
        }
        else
        {
            var documentExists = await _context.Employees.AnyAsync(e => e.DocumentNumber == request.DocumentNumber);
            if (documentExists) errors["DocumentNumber"] = new[] { EmployeeError.DocumentNumberAlreadyExists };
        }

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = new[] { EmployeeError.FirstNameRequired };
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = new[] { EmployeeError.LastNameRequired };
        
        var validArea = await _context.Departments.AnyAsync(d => d.Id == request.AreaId);
        if (!validArea) errors["AreaId"] = new[] { EmployeeError.InvalidArea };

        if (request.BranchId.HasValue)
        {
            var validBranch = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId.Value);
            if (!validBranch) errors["BranchId"] = new[] { EmployeeError.InvalidBranch };
        }

        var validPosition = await _context.Positions.AnyAsync(position => position.Id == request.PositionId);
        if (!validPosition) errors["PositionId"] = new[] { EmployeeError.InvalidPosition };

        var validSchedule = await _context.Schedules.AnyAsync(schedule => schedule.Id == request.ScheduleId);
        if (!validSchedule) errors["ScheduleId"] = new[] { EmployeeError.InvalidSchedule };

        if (request.BasicSalary <= 0) errors["BasicSalary"] = new[] { EmployeeError.BasicSalaryMustBeGreaterThanZero };
        if (request.PositionStartDate < request.HireDate) errors["PositionStartDate"] = new[] { EmployeeError.PositionStartDateBeforeHireDate };

        if (!Enum.IsDefined(typeof(BackEnd.Models.Employee.GenderEnum), request.Gender)) errors["Gender"] = new[] { EmployeeError.InvalidGender };

        if (!Enum.IsDefined(typeof(BackEnd.Models.Employee.MaritalStatusEnum), request.MaritalStatus)) errors["MaritalStatus"] = new[] { EmployeeError.InvalidMaritalStatus };

        if (request.InmediatlyBossId.HasValue)
        {
            var validBoss = await _context.Employees.AnyAsync(e => e.Id == request.InmediatlyBossId.Value);
            if (!validBoss) errors["InmediatlyBossId"] = new[] { EmployeeError.InvalidInmediatlyBoss };
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdateEmployeeRequestDto request, int currentEmployeeId)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = new[] { EmployeeError.DocumentNumberRequired };
        }
        else
        {
            var documentExists = await _context.Employees.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber && e.Id != currentEmployeeId);
            if (documentExists) errors["DocumentNumber"] = new[] { EmployeeError.DocumentNumberAlreadyExists };
        }

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = new[] { EmployeeError.FirstNameRequired };
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = new[] { EmployeeError.LastNameRequired };
        if (string.IsNullOrWhiteSpace(request.FileNumber)) errors["FileNumber"] = new[] { EmployeeError.FileNumberRequired };

        var validArea = await _context.Departments.AnyAsync(d => d.Id == request.AreaId);
        if (!validArea) errors["AreaId"] = new[] { EmployeeError.InvalidArea };

        if (request.BranchId.HasValue)
        {
            var validBranch = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId.Value);
            if (!validBranch) errors["BranchId"] = new[] { EmployeeError.InvalidBranch };
        }

        if (!Enum.IsDefined(typeof(BackEnd.Models.Employee.GenderEnum), request.Gender)) errors["Gender"] = new[] { EmployeeError.InvalidGender };

        if (!Enum.IsDefined(typeof(BackEnd.Models.Employee.MaritalStatusEnum), request.MaritalStatus)) errors["MaritalStatus"] = new[] { EmployeeError.InvalidMaritalStatus };

        if (request.InmediatlyBossId.HasValue)
        {
            var validBoss = await _context.Employees.AnyAsync(e => e.Id == request.InmediatlyBossId.Value);
            if (!validBoss) errors["InmediatlyBossId"] = new[] { EmployeeError.InvalidInmediatlyBoss };
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
        if (!validPosition) errors["PositionId"] = new[] { EmployeeError.InvalidPosition };

        var validSchedule = await _context.Schedules.AnyAsync(schedule => schedule.Id == request.ScheduleId);
        if (!validSchedule) errors["ScheduleId"] = new[] { EmployeeError.InvalidSchedule };

        if (request.BasicSalary <= 0) errors["BasicSalary"] = new[] { EmployeeError.BasicSalaryMustBeGreaterThanZero };

        if (request.StartDate < employee.HireDate) errors["StartDate"] = new[] { EmployeeError.PositionStartDateBeforeHireDate };

        var currentOpenHistory = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(history => history.EmployeeId == employeeId && history.EndDate == null)
            .OrderByDescending(history => history.StartDate)
            .ThenByDescending(history => history.Id)
            .FirstOrDefaultAsync();

        if (currentOpenHistory != null && request.StartDate <= currentOpenHistory.StartDate)
            errors["StartDate"] = new[] { EmployeeError.PositionStartDateMustBeAfterCurrent };

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

        if (!Enum.IsDefined(typeof(EmployeeRelation.RelationTypeEnum), request.RelationType))
            errors["RelationType"] = new[] { EmployeeError.FamilyRelationTypeInvalid };

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = new[] { EmployeeError.FamilyNameRequired };
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = new[] { EmployeeError.FamilyLastnameRequired };
        if (string.IsNullOrWhiteSpace(request.DocumentNumber)) errors["DocumentNumber"] = new[] { EmployeeError.FamilyDocumentRequired };

        var duplicatedDocument = await _context.EmployeeRelations.AsNoTracking().AnyAsync(relation =>
            relation.EmployeeId == employeeId && relation.DocumentNumber == request.DocumentNumber.Trim());

        if (duplicatedDocument) errors["DocumentNumber"] = new[] { EmployeeError.FamilyDocumentAlreadyExists };

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdatePositionHistoryRequestAsync(int employeeId, int historyId, UpdateEmployeePositionHistoryRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        var employee = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .Select(e => new { e.Id, e.HireDate })
            .FirstOrDefaultAsync();

        if (employee == null)
            return Result.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var history = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(h => h.Id == historyId && h.EmployeeId == employeeId)
            .Select(h => new { h.Id, h.StartDate })
            .FirstOrDefaultAsync();

        if (history == null)
            return Result.Failure(EmployeeError.PositionHistoryNotFound, ErrorType.NotFound);

        var latestHistory = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(h => h.EmployeeId == employeeId)
            .OrderByDescending(h => h.StartDate)
            .ThenByDescending(h => h.Id)
            .Select(h => new { h.Id })
            .FirstOrDefaultAsync();

        if (latestHistory == null || latestHistory.Id != historyId)
            return Result.Failure(EmployeeError.PositionHistoryNotEditable, ErrorType.Conflict);

        var validPosition = await _context.Positions.AnyAsync(position => position.Id == request.PositionId);
        if (!validPosition) errors["PositionId"] = new[] { EmployeeError.InvalidPosition };

        var validSchedule = await _context.Schedules.AnyAsync(schedule => schedule.Id == request.ScheduleId);
        if (!validSchedule) errors["ScheduleId"] = new[] { EmployeeError.InvalidSchedule };

        if (request.BasicSalary <= 0) errors["BasicSalary"] = new[] { EmployeeError.BasicSalaryMustBeGreaterThanZero };

        if (request.StartDate < employee.HireDate) errors["StartDate"] = new[] { EmployeeError.PositionStartDateBeforeHireDate };

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRelationRequestAsync(int employeeId, int relationId, UpdateEmployeeRelationRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        var relation = await _context.EmployeeRelations
            .AsNoTracking()
            .Where(r => r.Id == relationId && r.EmployeeId == employeeId)
            .Select(r => new { r.Id, r.RelationType })
            .FirstOrDefaultAsync();

        if (relation == null)
            return Result.Failure(EmployeeError.FamilyRelationNotFound, ErrorType.NotFound);

        if (!Enum.IsDefined(typeof(EmployeeRelation.RelationTypeEnum), request.RelationType))
            errors["RelationType"] = new[] { EmployeeError.FamilyRelationTypeInvalid };

        if (string.IsNullOrWhiteSpace(request.Name)) errors["Name"] = new[] { EmployeeError.FamilyNameRequired };
        if (string.IsNullOrWhiteSpace(request.Lastname)) errors["Lastname"] = new[] { EmployeeError.FamilyLastnameRequired };
        if (string.IsNullOrWhiteSpace(request.DocumentNumber)) errors["DocumentNumber"] = new[] { EmployeeError.FamilyDocumentRequired };

        var duplicatedDocument = await _context.EmployeeRelations.AsNoTracking().AnyAsync(rel =>
            rel.EmployeeId == employeeId && rel.DocumentNumber == request.DocumentNumber.Trim() && rel.Id != relationId);

        if (duplicatedDocument) errors["DocumentNumber"] = new[] { EmployeeError.FamilyDocumentAlreadyExists };

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }
}
