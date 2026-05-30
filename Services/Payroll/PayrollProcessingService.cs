using System.Globalization;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PayrollProcessingService(AppDbContext context, FormulaEvaluatorService formulaEvaluator)
{
    private readonly AppDbContext _context = context;
    private readonly FormulaEvaluatorService _formulaEvaluator = formulaEvaluator;

    public async Task<Result<ManualConceptIncidentResponseDto>> CreateManualConceptIncidentAsync(ManualConceptIncidentCreateDto request)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == request.EmployeeId);

        if (employee is null)
            return Result<ManualConceptIncidentResponseDto>.Failure("The requested employee was not found", ErrorType.NotFound);

        var payrollUpdate = await _context.PayrollUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(payrollUpdate => payrollUpdate.Id == request.PayrollUpdateId);

        if (payrollUpdate is null)
            return Result<ManualConceptIncidentResponseDto>.Failure(PayrollUpdateError.PayrollUpdateNotFound, ErrorType.NotFound);

        if (payrollUpdate.FormulaTypeId != PayrollUpdate.FormulaTypeEnum.Fixed)
            return Result<ManualConceptIncidentResponseDto>.Failure(ManualConceptIncidentError.ManualConceptMustBeFixed, ErrorType.Validation);

        var incident = new ManualConceptIncident
        {
            EmployeeId = request.EmployeeId,
            PayrollUpdateId = request.PayrollUpdateId,
            Amount = request.Amount,
            OccurrenceDate = request.OccurrenceDate,
            Status = ManualConceptIncident.ManualConceptStatus.Pending,
            PayrollProcessId = null
        };

        _context.ManualConceptIncidents.Add(incident);
        await _context.SaveChangesAsync();

        return Result<ManualConceptIncidentResponseDto>.Success(new ManualConceptIncidentResponseDto
        {
            Id = incident.Id,
            EmployeeId = employee.Id,
            EmployeeFullName = $"{employee.Name} {employee.Lastname}",
            PayrollUpdateId = payrollUpdate.Id,
            ConceptName = payrollUpdate.Name,
            PayrollTypeName = GetManualPayrollTypeName(payrollUpdate.PayrollTypeId),
            Amount = incident.Amount,
            OccurrenceDate = incident.OccurrenceDate,
            StatusName = nameof(ManualConceptIncident.ManualConceptStatus.Pending),
            PayrollProcessId = incident.PayrollProcessId
        });
    }

    public async Task<Result<List<ManualConceptIncidentResponseDto>>> GetPendingManualConceptIncidentsAsync()
    {
        var incidents = await _context.ManualConceptIncidents
            .AsNoTracking()
            .Where(incident => incident.Status == ManualConceptIncident.ManualConceptStatus.Pending)
            .OrderBy(incident => incident.OccurrenceDate)
            .ThenBy(incident => incident.Employee.Name)
            .ThenBy(incident => incident.PayrollUpdate.Name)
            .Select(incident => new ManualConceptIncidentResponseDto
            {
                Id = incident.Id,
                EmployeeId = incident.EmployeeId,
                EmployeeFullName = incident.Employee.Name + " " + incident.Employee.Lastname,
                PayrollUpdateId = incident.PayrollUpdateId,
                ConceptName = incident.PayrollUpdate.Name,
                PayrollTypeName = GetManualPayrollTypeName(incident.PayrollUpdate.PayrollTypeId),
                Amount = incident.Amount,
                OccurrenceDate = incident.OccurrenceDate,
                StatusName = nameof(ManualConceptIncident.ManualConceptStatus.Pending),
                PayrollProcessId = incident.PayrollProcessId
            })
            .ToListAsync();

        return Result<List<ManualConceptIncidentResponseDto>>.Success(incidents);
    }

    public async Task<Result> UpdatePayrollProcessStatusAsync(int payrollProcessId, UpdatePayrollProcessStatusRequestDto request)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(payrollProcess => payrollProcess.Id == payrollProcessId);
        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var status = await _context.PayrollStatuses.FirstOrDefaultAsync(status => status.Id == request.PayrollStatusId);
        if (status is null)
            return Result.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.NotFound);

        process.PayrollStatusId = status.Id;

        if (IsFinalPayrollStatus(status.Name))
        {
            await AssignPendingManualConceptIncidentsAsync(process.Id);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<PayrollProcessResponseDto>>> GetListAsync()
    {
        var processes = await _context.PayrollProcesses
            .AsNoTracking()
            .Include(p => p.PayrollStatus)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Select(p => new PayrollProcessResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                ProcessTypeId = (int)p.ProcessTypeId,
                ProcessTypeName = p.ProcessTypeId.ToString(),
                Year = p.Year,
                Month = p.Month,
                StartDate = p.StartDate,
                PayDate = p.PayDate,
                PayrollStatusId = p.PayrollStatusId,
                PayrollStatusName = p.PayrollStatus.Name
            })
            .ToListAsync();

        return Result<List<PayrollProcessResponseDto>>.Success(processes);
    }

    public async Task<Result<PayrollProcessResponseDto>> GetByIdAsync(int id)
    {
        var p = await _context.PayrollProcesses
            .AsNoTracking()
            .Include(x => x.PayrollStatus)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null)
            return Result<PayrollProcessResponseDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var dto = new PayrollProcessResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            ProcessTypeId = (int)p.ProcessTypeId,
            ProcessTypeName = p.ProcessTypeId.ToString(),
            Year = p.Year,
            Month = p.Month,
            StartDate = p.StartDate,
            PayDate = p.PayDate,
            PayrollStatusId = p.PayrollStatusId,
            PayrollStatusName = p.PayrollStatus.Name
        };

        return Result<PayrollProcessResponseDto>.Success(dto);
    }

    public async Task<Result<PayrollProcessResponseDto>> CreatePayrollProcessAsync(PayrollProcessCreateDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = new[] { "Name is required" };

        if (request.Month < 1 || request.Month > 12)
            errors["Month"] = new[] { "Month must be between 1 and 12" };

        if (request.Year < 1900)
            errors["Year"] = new[] { "Year is invalid" };

        if (!Enum.IsDefined(typeof(PayrollProcess.ProcessTypeEnum), request.ProcessTypeId))
            errors["ProcessTypeId"] = new[] { "Invalid process type" };

        if (errors.Count > 0)
            return Result<PayrollProcessResponseDto>.Failure(string.Join("; ", errors.SelectMany(e => e.Value)), errors, ErrorType.Validation);

        int statusId;
        if (request.PayrollStatusId.HasValue)
        {
            var status = await _context.PayrollStatuses.FirstOrDefaultAsync(s => s.Id == request.PayrollStatusId.Value);
            if (status is null)
                return Result<PayrollProcessResponseDto>.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.NotFound);
            statusId = status.Id;
        }
        else
        {
            var openId = await GetPayrollStatusIdAsync("Abierto");
            if (openId is null)
                return Result<PayrollProcessResponseDto>.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.Failure);
            statusId = openId.Value;
        }

        var process = new PayrollProcess
        {
            Name = request.Name.Trim(),
            ProcessTypeId = (PayrollProcess.ProcessTypeEnum)request.ProcessTypeId,
            Year = request.Year,
            Month = request.Month,
            StartDate = request.StartDate,
            PayDate = request.PayDate,
            PayrollStatusId = statusId
        };

        _context.PayrollProcesses.Add(process);
        await _context.SaveChangesAsync();

        var created = await GetByIdAsync(process.Id);
        return created;
    }

    public async Task<Result> UpdatePayrollProcessAsync(int id, PayrollProcessUpdateDto request)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(p => p.Id == id);
        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (!Enum.IsDefined(typeof(PayrollProcess.ProcessTypeEnum), request.ProcessTypeId))
            return Result.Failure("Invalid process type", ErrorType.Validation);

        if (request.Month < 1 || request.Month > 12)
            return Result.Failure("Month must be between 1 and 12", ErrorType.Validation);

        if (request.Year < 1900)
            return Result.Failure("Year is invalid", ErrorType.Validation);

        process.Name = request.Name.Trim();
        process.ProcessTypeId = (PayrollProcess.ProcessTypeEnum)request.ProcessTypeId;
        process.Year = request.Year;
        process.Month = request.Month;
        process.StartDate = request.StartDate;
        process.PayDate = request.PayDate;

        if (request.PayrollStatusId.HasValue)
        {
            var status = await _context.PayrollStatuses.FirstOrDefaultAsync(s => s.Id == request.PayrollStatusId.Value);
            if (status is null)
                return Result.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.NotFound);
            process.PayrollStatusId = status.Id;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeletePayrollProcessAsync(int id)
    {
        var process = await _context.PayrollProcesses.Include(p => p.PayrollStatus).FirstOrDefaultAsync(p => p.Id == id);
        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (IsFinalPayrollStatus(process.PayrollStatus.Name))
            return Result.Failure("Cannot delete a payroll process in final status", ErrorType.Conflict);

        _context.PayrollProcesses.Remove(process);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<PayrollManualDetailResponseDto>> UpsertManualDetailAsync(int payrollProcessId, PayrollManualInputDto request)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(payrollProcess => payrollProcess.Id == payrollProcessId);
        if (process is null)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var openStatusId = await GetPayrollStatusIdAsync("Abierto");
        if (openStatusId is null)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.Failure);

        if (process.PayrollStatusId != openStatusId.Value)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == request.EmployeeId);

        if (employee is null)
            return Result<PayrollManualDetailResponseDto>.Failure("The requested employee was not found", ErrorType.NotFound);

        var payrollUpdate = await _context.PayrollUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(payrollUpdate => payrollUpdate.Id == request.PayrollUpdateId);

        if (payrollUpdate is null)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollUpdateError.PayrollUpdateNotFound, ErrorType.NotFound);

        if (payrollUpdate.FormulaTypeId != PayrollUpdate.FormulaTypeEnum.Fixed)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollManualDetailError.PayrollUpdateMustBeFixed, ErrorType.Validation);

        var existingDetail = await _context.PayrollProcessDetails
            .FirstOrDefaultAsync(detail =>
                detail.PayrollProcessId == payrollProcessId &&
                detail.EmployeeId == request.EmployeeId &&
                detail.PayrollUpdateId == request.PayrollUpdateId);

        if (existingDetail is null)
        {
            existingDetail = new PayrollProcessDetail
            {
                PayrollProcessId = payrollProcessId,
                EmployeeId = request.EmployeeId,
                PayrollUpdateId = request.PayrollUpdateId,
                Amount = request.Amount
            };

            _context.PayrollProcessDetails.Add(existingDetail);
        }
        else
        {
            existingDetail.Amount = request.Amount;
        }

        await _context.SaveChangesAsync();

        return Result<PayrollManualDetailResponseDto>.Success(new PayrollManualDetailResponseDto
        {
            Id = existingDetail.Id,
            EmployeeId = employee.Id,
            EmployeeFullName = $"{employee.Name} {employee.Lastname}",
            ConceptName = payrollUpdate.Name,
            PayrollTypeName = GetManualPayrollTypeName(payrollUpdate.PayrollTypeId),
            Amount = existingDetail.Amount
        });
    }

    public async Task<Result<List<PayrollManualDetailResponseDto>>> GetManualDetailsAsync(int payrollProcessId)
    {
        var process = await _context.PayrollProcesses.AsNoTracking().FirstOrDefaultAsync(payrollProcess => payrollProcess.Id == payrollProcessId);
        if (process is null)
            return Result<List<PayrollManualDetailResponseDto>>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var manualDetails = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Where(detail => detail.PayrollProcessId == payrollProcessId && detail.PayrollUpdate.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Fixed)
            .OrderBy(detail => detail.Employee.Name)
            .ThenBy(detail => detail.Employee.Lastname)
            .ThenBy(detail => detail.PayrollUpdate.Name)
            .Select(detail => new PayrollManualDetailResponseDto
            {
                Id = detail.Id,
                EmployeeId = detail.EmployeeId,
                EmployeeFullName = detail.Employee.Name + " " + detail.Employee.Lastname,
                ConceptName = detail.PayrollUpdate.Name,
                PayrollTypeName = GetManualPayrollTypeName(detail.PayrollUpdate.PayrollTypeId),
                Amount = detail.Amount
            })
            .ToListAsync();

        return Result<List<PayrollManualDetailResponseDto>>.Success(manualDetails);
    }

    public async Task<Result> DeleteManualDetailAsync(int id)
    {
        var detail = await _context.PayrollProcessDetails
            .Include(payrollProcessDetail => payrollProcessDetail.PayrollProcess)
            .Include(payrollProcessDetail => payrollProcessDetail.PayrollUpdate)
            .FirstOrDefaultAsync(payrollProcessDetail => payrollProcessDetail.Id == id);

        if (detail is null)
            return Result.Failure(PayrollManualDetailError.ManualDetailNotFound, ErrorType.NotFound);

        if (detail.PayrollUpdate.FormulaTypeId != PayrollUpdate.FormulaTypeEnum.Fixed)
            return Result.Failure(PayrollManualDetailError.PayrollUpdateMustBeFixed, ErrorType.Validation);

        var openStatusId = await GetPayrollStatusIdAsync("Abierto");
        if (openStatusId is null)
            return Result.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.Failure);

        if (detail.PayrollProcess.PayrollStatusId != openStatusId.Value)
            return Result.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        _context.PayrollProcessDetails.Remove(detail);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<PayrollProcessCalculationResponseDto>> CalculateAsync(int payrollProcessId)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(payrollProcess => payrollProcess.Id == payrollProcessId);
        if (process is null)
            return Result<PayrollProcessCalculationResponseDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var openStatusId = await GetPayrollStatusIdAsync("Abierto");
        var processedStatusId = await GetPayrollStatusIdAsync("Procesado");

        if (openStatusId is null || processedStatusId is null)
            return Result<PayrollProcessCalculationResponseDto>.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.Failure);

        if (process.PayrollStatusId != openStatusId.Value)
            return Result<PayrollProcessCalculationResponseDto>.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var payrollUpdates = await _context.PayrollUpdates
            .AsNoTracking()
            .OrderBy(payrollUpdate => payrollUpdate.Id)
            .ToListAsync();

        var earningsUpdates = payrollUpdates
            .Where(payrollUpdate => payrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
            .OrderBy(payrollUpdate => payrollUpdate.Id)
            .ToList();

        var deductionsUpdates = payrollUpdates
            .Where(payrollUpdate => payrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions)
            .OrderBy(payrollUpdate => payrollUpdate.FormulaTypeId)
            .ThenBy(payrollUpdate => payrollUpdate.Id)
            .ToList();

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.Id)
            .ToListAsync();

        var employeeIds = employees.Select(employee => employee.Id).ToList();

        var pendingIncidents = await _context.ManualConceptIncidents
            .Where(incident =>
                incident.Status == ManualConceptIncident.ManualConceptStatus.Pending &&
                employeeIds.Contains(incident.EmployeeId) &&
                (incident.PayrollProcessId == null || incident.PayrollProcessId == process.Id))
            .OrderBy(incident => incident.EmployeeId)
            .ThenBy(incident => incident.PayrollUpdateId)
            .ThenBy(incident => incident.OccurrenceDate)
            .ThenBy(incident => incident.Id)
            .ToListAsync();

        var incidentsByEmployeeAndUpdate = pendingIncidents
            .GroupBy(incident => (incident.EmployeeId, incident.PayrollUpdateId))
            .ToDictionary(group => group.Key, group => group.ToList());

        var existingDetails = await _context.PayrollProcessDetails
            .Where(detail => detail.PayrollProcessId == process.Id)
            .ToListAsync();

        var detailsByKey = existingDetails.ToDictionary(detail => (detail.EmployeeId, detail.PayrollUpdateId));

        var periodStart = new DateOnly(process.Year, process.Month, 1);
        var periodEnd = new DateOnly(process.Year, process.Month, DateTime.DaysInMonth(process.Year, process.Month));
        var referenceDate = process.PayDate ?? periodEnd;

        var response = new PayrollProcessCalculationResponseDto
        {
            PayrollProcessId = process.Id,
            PayrollProcessName = process.Name
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var employee in employees)
            {
                var salarioBase = await ResolveSalaryBaseAsync(employee, referenceDate);
                var jornalDiario = decimal.Round(salarioBase / 30m, 2, MidpointRounding.AwayFromZero);
                var diasTrabajados = await ResolveDaysWorkedAsync(employee.Id, periodStart, periodEnd);
                var cantidadHijos = await ResolveChildrenCountAsync(employee.Id, referenceDate);

                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["SalarioBase"] = salarioBase,
                    ["JornalDiario"] = jornalDiario,
                    ["DiasTrabajados"] = diasTrabajados,
                    ["CantidadHijos"] = cantidadHijos,
                    ["TotalDeducibleIPS"] = 0m
                };

                var totalDeducibleIPS = 0m;
                var totalHaberes = 0m;
                var totalDescuentos = 0m;
                var employeeDetails = new List<PayrollProcessDetailCalculationResponseDto>();

                foreach (var update in earningsUpdates)
                {
                    var amount = await ResolvePayrollUpdateAmountAsync(process.Id, employee, update, variables, detailsByKey, incidentsByEmployeeAndUpdate);

                    totalHaberes += amount;

                    if (update.IpsDeductible)
                        totalDeducibleIPS += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);

                    employeeDetails.Add(CreateDetailResponse(update, amount));
                }

                variables["TotalDeducibleIPS"] = totalDeducibleIPS;

                foreach (var update in deductionsUpdates)
                {
                    var amount = await ResolvePayrollUpdateAmountAsync(process.Id, employee, update, variables, detailsByKey, incidentsByEmployeeAndUpdate);

                    totalDescuentos += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);

                    employeeDetails.Add(CreateDetailResponse(update, amount));
                }

                response.Employees.Add(new PayrollEmployeeCalculationResponseDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = $"{employee.Name} {employee.Lastname}",
                    SalarioBase = salarioBase,
                    JornalDiario = jornalDiario,
                    DiasTrabajados = diasTrabajados,
                    CantidadHijos = cantidadHijos,
                    TotalDeducibleIPS = totalDeducibleIPS,
                    TotalHaberes = totalHaberes,
                    TotalDescuentos = totalDescuentos,
                    TotalNeto = totalHaberes - totalDescuentos,
                    Details = employeeDetails
                });

                response.TotalHaberes += totalHaberes;
                response.TotalDescuentos += totalDescuentos;
                response.TotalNeto += totalHaberes - totalDescuentos;
            }

            response.EmployeesProcessed = response.Employees.Count;

            process.PayrollStatusId = processedStatusId.Value;

            await LinkPendingIncidentsToProcessAsync(process.Id, pendingIncidents);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result<PayrollProcessCalculationResponseDto>.Success(response);
        }
        catch (InvalidOperationException exception)
        {
            await transaction.RollbackAsync();
            return Result<PayrollProcessCalculationResponseDto>.Failure(exception.Message, ErrorType.Conflict);
        }
        catch (KeyNotFoundException exception)
        {
            await transaction.RollbackAsync();
            return Result<PayrollProcessCalculationResponseDto>.Failure(exception.Message, ErrorType.Validation);
        }
        catch (FormatException exception)
        {
            await transaction.RollbackAsync();
            return Result<PayrollProcessCalculationResponseDto>.Failure(exception.Message, ErrorType.Validation);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<decimal> ResolvePayrollUpdateAmountAsync(
        int payrollProcessId,
        Employee employee,
        PayrollUpdate payrollUpdate,
        Dictionary<string, decimal> variables,
        Dictionary<(int EmployeeId, int PayrollUpdateId), PayrollProcessDetail> detailsByKey,
        Dictionary<(int EmployeeId, int PayrollUpdateId), List<ManualConceptIncident>> incidentsByEmployeeAndUpdate)
    {
        var key = (employee.Id, payrollUpdate.Id);
        var existingDetail = detailsByKey.TryGetValue(key, out var detail) ? detail : null;

        if (payrollUpdate.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Fixed)
        {
            if (!incidentsByEmployeeAndUpdate.TryGetValue(key, out var incidents) || incidents.Count == 0)
                throw new InvalidOperationException($"{PayrollProcessError.ManualAmountRequired} ({payrollUpdate.Name} - {employee.Name} {employee.Lastname})");

            var manualAmount = incidents.Sum(incident => incident.Amount);

            if (existingDetail is null)
            {
                existingDetail = new PayrollProcessDetail
                {
                    PayrollProcessId = payrollProcessId,
                    EmployeeId = employee.Id,
                    PayrollUpdateId = payrollUpdate.Id,
                    Amount = manualAmount
                };

                _context.PayrollProcessDetails.Add(existingDetail);
                detailsByKey[key] = existingDetail;
            }
            else
            {
                existingDetail.Amount = manualAmount;
            }

            foreach (var incident in incidents)
                incident.PayrollProcessId = payrollProcessId;

            return manualAmount;
        }

        if (string.IsNullOrWhiteSpace(payrollUpdate.Formula))
            throw new InvalidOperationException($"{PayrollProcessError.ManualAmountRequired} ({payrollUpdate.Name})");

        var amount = _formulaEvaluator.EvaluateFormula(payrollUpdate.Formula, variables);

        if (existingDetail is null)
        {
            existingDetail = new PayrollProcessDetail
            {
                PayrollProcessId = payrollProcessId,
                EmployeeId = employee.Id,
                PayrollUpdateId = payrollUpdate.Id,
                Amount = amount
            };

            _context.PayrollProcessDetails.Add(existingDetail);
            detailsByKey[key] = existingDetail;
        }
        else
        {
            existingDetail.Amount = amount;
        }

        return amount;
    }

    private async Task LinkPendingIncidentsToProcessAsync(int payrollProcessId, List<ManualConceptIncident> pendingIncidents)
    {
        if (pendingIncidents.Count == 0)
            return;

        foreach (var incident in pendingIncidents)
        {
            incident.PayrollProcessId = payrollProcessId;
        }

        await Task.CompletedTask;
    }

    private async Task AssignPendingManualConceptIncidentsAsync(int payrollProcessId)
    {
        var incidents = await _context.ManualConceptIncidents
            .Where(incident => incident.PayrollProcessId == payrollProcessId && incident.Status == ManualConceptIncident.ManualConceptStatus.Pending)
            .ToListAsync();

        foreach (var incident in incidents)
        {
            incident.Status = ManualConceptIncident.ManualConceptStatus.Assigned;
        }
    }

    private static bool IsFinalPayrollStatus(string statusName)
    {
        return statusName.Equals("Cerrado", StringComparison.OrdinalIgnoreCase)
            || statusName.Equals("Pagado", StringComparison.OrdinalIgnoreCase);
    }

    private void UpsertDetail(
        int payrollProcessId,
        int employeeId,
        int payrollUpdateId,
        decimal amount,
        Dictionary<(int EmployeeId, int PayrollUpdateId), PayrollProcessDetail> detailsByKey)
    {
        var key = (employeeId, payrollUpdateId);

        if (detailsByKey.TryGetValue(key, out var existingDetail))
        {
            existingDetail.Amount = amount;
            return;
        }

        var detail = new PayrollProcessDetail
        {
            PayrollProcessId = payrollProcessId,
            EmployeeId = employeeId,
            PayrollUpdateId = payrollUpdateId,
            Amount = amount
        };

        _context.PayrollProcessDetails.Add(detail);
        detailsByKey[key] = detail;
    }

    private static PayrollProcessDetailCalculationResponseDto CreateDetailResponse(PayrollUpdate payrollUpdate, decimal amount)
    {
        return new PayrollProcessDetailCalculationResponseDto
        {
            PayrollUpdateId = payrollUpdate.Id,
            PayrollUpdateName = payrollUpdate.Name,
            PayrollTypeId = (int)payrollUpdate.PayrollTypeId,
            FormulaTypeId = (int)payrollUpdate.FormulaTypeId,
            Amount = amount
        };
    }

    private async Task<int?> GetPayrollStatusIdAsync(string statusName)
    {
        return await _context.PayrollStatuses
            .AsNoTracking()
            .Where(status => status.Name.ToLower() == statusName.ToLower())
            .Select(status => (int?)status.Id)
            .FirstOrDefaultAsync();
    }

    private static string GetManualPayrollTypeName(PayrollUpdate.PayrollTypeEnum payrollTypeId)
    {
        return payrollTypeId switch
        {
            PayrollUpdate.PayrollTypeEnum.Earnings => "Haber",
            PayrollUpdate.PayrollTypeEnum.Deductions => "Descuento",
            _ => "Unknown"
        };
    }

    private async Task<decimal> ResolveSalaryBaseAsync(Employee employee, DateOnly referenceDate)
    {
        var assignment = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(positionByScheduleByEmployee =>
                positionByScheduleByEmployee.EmployeeId == employee.Id &&
                positionByScheduleByEmployee.StartDate <= referenceDate &&
                (positionByScheduleByEmployee.EndDate == null || positionByScheduleByEmployee.EndDate >= referenceDate))
            .OrderByDescending(positionByScheduleByEmployee => positionByScheduleByEmployee.StartDate)
            .ThenByDescending(positionByScheduleByEmployee => positionByScheduleByEmployee.Id)
            .Include(positionByScheduleByEmployee => positionByScheduleByEmployee.Position)
            .FirstOrDefaultAsync();

        if (assignment is null)
            throw new InvalidOperationException($"{PayrollProcessError.MissingPositionAssignment}: {employee.Name} {employee.Lastname}");

        return assignment.BasicSalary > 0m ? assignment.BasicSalary : assignment.Position.DefaultBasicSalary;
    }

    private async Task<decimal> ResolveDaysWorkedAsync(int employeeId, DateOnly periodStart, DateOnly periodEnd)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Include(attendance => attendance.AttendanceType)
            .Where(attendance =>
                attendance.EmployeeId == employeeId &&
                attendance.Date >= periodStart &&
                attendance.Date <= periodEnd &&
                attendance.AttendanceType.AffectsPayroll)
            .CountAsync();
    }

    private async Task<decimal> ResolveChildrenCountAsync(int employeeId, DateOnly referenceDate)
    {
        var adultThreshold = referenceDate.AddYears(-18);

        return await _context.EmployeeRelations
            .AsNoTracking()
            .Where(relation =>
                relation.EmployeeId == employeeId &&
                relation.RelationType == EmployeeRelation.RelationTypeEnum.Child &&
                relation.BirthDate > adultThreshold &&
                relation.StartDate <= referenceDate &&
                (relation.EndDate == null || relation.EndDate >= referenceDate))
            .CountAsync();
    }
}