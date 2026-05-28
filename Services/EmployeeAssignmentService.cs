using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Employee;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class EmployeeAssignmentService(AppDbContext context) : IEmployeeAssignmentService
{
    private readonly AppDbContext _context = context;

    public async Task<Result<PositionByScheduleByEmployeeDto>> AssignPositionAndScheduleAsync(PositionByScheduleByEmployeeDto dto)
    {
        var validation = await ValidateAsync(dto);
        if (!validation.IsSuccess)
            return Result<PositionByScheduleByEmployeeDto>.Failure(validation.ErrorMessage!, validation.Errors!, validation.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            PositionByScheduleByEmployee assignment;

            if (dto.Id > 0)
            {
                assignment = await _context.PositionByScheduleByEmployees
                    .FirstOrDefaultAsync(history => history.Id == dto.Id && history.EmployeeId == dto.EmployeeId)
                    ?? throw new InvalidOperationException(EmployeeError.PositionHistoryNotFound);

                assignment.PositionId = dto.PositionId;
                assignment.ScheduleId = dto.ScheduleId;
                assignment.BasicSalary = dto.BasicSalary;
                assignment.StartDate = dto.StartDate;
                assignment.EndDate = dto.EndDate;
            }
            else
            {
                var currentOpenHistory = await _context.PositionByScheduleByEmployees
                    .Where(history => history.EmployeeId == dto.EmployeeId && history.EndDate == null)
                    .OrderByDescending(history => history.StartDate)
                    .ThenByDescending(history => history.Id)
                    .FirstOrDefaultAsync();

                if (currentOpenHistory != null)
                {
                    if (dto.StartDate <= currentOpenHistory.StartDate)
                        return Result<PositionByScheduleByEmployeeDto>.Failure(EmployeeError.PositionStartDateMustBeAfterCurrent, ErrorType.Validation);

                    currentOpenHistory.EndDate = dto.StartDate.AddDays(-1);
                }

                assignment = new PositionByScheduleByEmployee
                {
                    EmployeeId = dto.EmployeeId,
                    PositionId = dto.PositionId,
                    ScheduleId = dto.ScheduleId,
                    BasicSalary = dto.BasicSalary,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                };

                _context.PositionByScheduleByEmployees.Add(assignment);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result<PositionByScheduleByEmployeeDto>.Success(MapToDto(assignment));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Result> ValidateAsync(PositionByScheduleByEmployeeDto dto)
    {
        if (dto.EmployeeId <= 0)
            return Result.Failure(EmployeeError.EmployeeNotFound, CreateFieldError(nameof(dto.EmployeeId), EmployeeError.EmployeeNotFound), ErrorType.Validation);

        if (dto.PositionId <= 0)
            return Result.Failure(EmployeeError.InvalidPosition, CreateFieldError(nameof(dto.PositionId), EmployeeError.InvalidPosition), ErrorType.Validation);

        if (dto.ScheduleId <= 0)
            return Result.Failure(EmployeeError.InvalidSchedule, CreateFieldError(nameof(dto.ScheduleId), EmployeeError.InvalidSchedule), ErrorType.Validation);

        if (dto.BasicSalary <= 0)
            return Result.Failure(EmployeeError.BasicSalaryMustBeGreaterThanZero, CreateFieldError(nameof(dto.BasicSalary), EmployeeError.BasicSalaryMustBeGreaterThanZero), ErrorType.Validation);

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            return Result.Failure(EmployeeError.PositionEndDateInvalid, CreateFieldError(nameof(dto.EndDate), EmployeeError.PositionEndDateInvalid), ErrorType.Validation);

        var employeeExists = await _context.Employees.AnyAsync(employee => employee.Id == dto.EmployeeId);
        if (!employeeExists)
            return Result.Failure(EmployeeError.EmployeeNotFound, CreateFieldError(nameof(dto.EmployeeId), EmployeeError.EmployeeNotFound), ErrorType.NotFound);

        var positionExists = await _context.Positions.AnyAsync(position => position.Id == dto.PositionId);
        if (!positionExists)
            return Result.Failure(EmployeeError.InvalidPosition, CreateFieldError(nameof(dto.PositionId), EmployeeError.InvalidPosition), ErrorType.NotFound);

        var scheduleExists = await _context.Schedules.AnyAsync(schedule => schedule.Id == dto.ScheduleId);
        if (!scheduleExists)
            return Result.Failure(EmployeeError.InvalidSchedule, CreateFieldError(nameof(dto.ScheduleId), EmployeeError.InvalidSchedule), ErrorType.NotFound);

        if (dto.Id > 0)
        {
            var assignmentExists = await _context.PositionByScheduleByEmployees
                .AnyAsync(history => history.Id == dto.Id && history.EmployeeId == dto.EmployeeId);

            if (!assignmentExists)
                return Result.Failure(EmployeeError.PositionHistoryNotFound, CreateFieldError(nameof(dto.Id), EmployeeError.PositionHistoryNotFound), ErrorType.NotFound);
        }

        return Result.Success();
    }

    private static Dictionary<string, string[]> CreateFieldError(string field, string message)
        => new() { [field] = [message] };

    private static PositionByScheduleByEmployeeDto MapToDto(PositionByScheduleByEmployee entity)
        => new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PositionId = entity.PositionId,
            ScheduleId = entity.ScheduleId,
            BasicSalary = entity.BasicSalary,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate
        };
}