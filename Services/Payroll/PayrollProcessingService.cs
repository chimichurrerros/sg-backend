using System.Globalization;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Entry;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PayrollProcess;
using BackEnd.DTOs.Responses.PayrollProcess;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PayrollProcessingService(AppDbContext context, FormulaEvaluatorService formulaEvaluator, EntryService entryService)
{
    private const decimal SueldoMinimo = 2899048m;

    private readonly AppDbContext _context = context;
    private readonly FormulaEvaluatorService _formulaEvaluator = formulaEvaluator;
    private readonly EntryService _entryService = entryService;

    public async Task<Result<ManualConceptIncidentResponseDto>> CreateManualConceptIncidentAsync(ManualConceptIncidentCreateDto request)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == request.EmployeeId);

        if (employee is null)
            return Result<ManualConceptIncidentResponseDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

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

    public async Task<Result<ManualConceptIncidentResponseDto>> UpdateManualConceptIncidentAsync(int id, ManualConceptIncidentCreateDto request)
    {
        var incident = await _context.ManualConceptIncidents
            .Include(i => i.Employee)
            .Include(i => i.PayrollUpdate)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident is null)
            return Result<ManualConceptIncidentResponseDto>.Failure(ManualConceptIncidentError.ManualConceptIncidentNotFound, ErrorType.NotFound);

        if (incident.Status != ManualConceptIncident.ManualConceptStatus.Pending)
            return Result<ManualConceptIncidentResponseDto>.Failure("Solo se pueden editar novedades en estado Pendiente", ErrorType.Conflict);

        var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
        if (employee is null)
            return Result<ManualConceptIncidentResponseDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var payrollUpdate = await _context.PayrollUpdates.AsNoTracking().FirstOrDefaultAsync(pu => pu.Id == request.PayrollUpdateId);
        if (payrollUpdate is null)
            return Result<ManualConceptIncidentResponseDto>.Failure(PayrollUpdateError.PayrollUpdateNotFound, ErrorType.NotFound);

        if (payrollUpdate.FormulaTypeId != PayrollUpdate.FormulaTypeEnum.Fixed)
            return Result<ManualConceptIncidentResponseDto>.Failure(ManualConceptIncidentError.ManualConceptMustBeFixed, ErrorType.Validation);

        incident.EmployeeId = request.EmployeeId;
        incident.PayrollUpdateId = request.PayrollUpdateId;
        incident.Amount = request.Amount;
        incident.OccurrenceDate = request.OccurrenceDate;

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

    public async Task<Result> DeleteManualConceptIncidentAsync(int id)
    {
        var incident = await _context.ManualConceptIncidents
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident is null)
            return Result.Failure(ManualConceptIncidentError.ManualConceptIncidentNotFound, ErrorType.NotFound);

        if (incident.Status != ManualConceptIncident.ManualConceptStatus.Pending)
            return Result.Failure("Solo se pueden eliminar novedades en estado Pendiente", ErrorType.Conflict);

        _context.ManualConceptIncidents.Remove(incident);
        await _context.SaveChangesAsync();

        return Result.Success();
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

        if (!Enum.IsDefined(typeof(PayrollProcess.PayrollStatusEnum), request.PayrollStatusId))
            return Result.Failure(PayrollProcessError.PayrollProcessStatusNotFound, ErrorType.NotFound);

        var newStatus = (PayrollProcess.PayrollStatusEnum)request.PayrollStatusId;
        process.PayrollStatusId = newStatus;

        if (IsFinalPayrollStatus(newStatus))
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
                ClosedAt = p.ClosedAt,
                PaidAt = p.PaidAt,
                PayrollStatusId = (int)p.PayrollStatusId,
                PayrollStatusName = p.PayrollStatusId.ToString()
            })
            .ToListAsync();

        return Result<List<PayrollProcessResponseDto>>.Success(processes);
    }

    public async Task<Result<PayrollProcessResponseDto>> GetByIdAsync(int id)
    {
        var p = await _context.PayrollProcesses
            .AsNoTracking()
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
            ClosedAt = p.ClosedAt,
            PaidAt = p.PaidAt,
            PayrollStatusId = (int)p.PayrollStatusId,
            PayrollStatusName = p.PayrollStatusId.ToString()
        };

        return Result<PayrollProcessResponseDto>.Success(dto);
    }

    public async Task<Result<List<EligibleEmployeeResponseDto>>> GetEligibleEmployeesAsync(int processId)
    {
        var process = await _context.PayrollProcesses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process is null)
            return Result<List<EligibleEmployeeResponseDto>>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var employeeIdsInProcess = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Where(d => d.PayrollProcessId == processId)
            .Select(d => d.EmployeeId)
            .Distinct()
            .ToListAsync();

        var referenceDate = process.PayDate ?? new DateOnly(process.Year, process.Month, DateTime.DaysInMonth(process.Year, process.Month));

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(e => e.IsActive && !employeeIdsInProcess.Contains(e.Id))
            .Include(e => e.Branch)
            .Include(e => e.Area)
            .Include(e => e.PositionByScheduleByEmployees.Where(psbe =>
                psbe.StartDate <= referenceDate &&
                (psbe.EndDate == null || psbe.EndDate >= referenceDate)))
                .ThenInclude(psbe => psbe.Position)
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Lastname)
            .ToListAsync();

        var result = employees.Select(e =>
        {
            var currentPosition = e.PositionByScheduleByEmployees
                .OrderByDescending(psbe => psbe.StartDate)
                .ThenByDescending(psbe => psbe.Id)
                .FirstOrDefault();

            return new EligibleEmployeeResponseDto
            {
                Id = e.Id,
                FileNumber = e.FileNumber,
                FirstName = e.Name,
                LastName = e.Lastname,
                BranchName = e.Branch?.Name,
                AreaName = e.Area?.Name,
                PositionName = currentPosition?.Position?.Name
            };
        }).ToList();

        return Result<List<EligibleEmployeeResponseDto>>.Success(result);
    }

    public async Task<Result<int>> AddEmployeesAsync(int processId, int[] employeeIds)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(payrollProcess => payrollProcess.Id == processId);
        if (process is null)
            return Result<int>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
            return Result<int>.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var payrollUpdates = await _context.PayrollUpdates
            .AsNoTracking()
            .OrderBy(payrollUpdate => payrollUpdate.Id)
            .ToListAsync();

        var calculatedEarnings = payrollUpdates
            .Where(pu => pu.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings
                      && pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Calculated)
            .OrderBy(pu => pu.Id)
            .ToList();

        var manualUpdates = payrollUpdates
            .Where(pu => pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Fixed)
            .OrderBy(pu => pu.Id)
            .ToList();

        var calculatedDeductions = payrollUpdates
            .Where(pu => pu.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions
                      && pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Calculated)
            .OrderBy(pu => pu.Id)
            .ToList();

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive && employeeIds.Contains(employee.Id))
            .OrderBy(employee => employee.Id)
            .ToListAsync();

        if (employees.Count == 0)
            return Result<int>.Failure("No se encontraron empleados activos con los IDs proporcionados.", ErrorType.NotFound);

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
            .Where(detail => detail.PayrollProcessId == process.Id && employeeIds.Contains(detail.EmployeeId))
            .ToListAsync();

        var detailsByKey = existingDetails.ToDictionary(detail => (detail.EmployeeId, detail.PayrollUpdateId));

        var periodEnd = new DateOnly(process.Year, process.Month, DateTime.DaysInMonth(process.Year, process.Month));
        var referenceDate = process.PayDate ?? periodEnd;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var addedCount = 0;

            foreach (var employee in employees)
            {
                var salarioBase = await ResolveSalaryBaseAsync(employee, referenceDate);
                var jornalDiario = decimal.Round(salarioBase / 30m, 2, MidpointRounding.AwayFromZero);
                var attendanceData = await ResolveAttendanceDataAsync(employee.Id, process.Year, process.Month);
                var cantidadHijos = await ResolveChildrenCountAsync(employee.Id, referenceDate);

                var aniosAntiguedad = process.Year - employee.HireDate.Year;
                if (employee.HireDate.Month > process.Month ||
                    (employee.HireDate.Month == process.Month && employee.HireDate.Day > DateTime.DaysInMonth(process.Year, process.Month)))
                    aniosAntiguedad--;

                var sueldoMinimo = 2899048m;
                var valorHoraOrdinaria = decimal.Round(jornalDiario / 8m, 2, MidpointRounding.AwayFromZero);
                var horasTardanza = attendanceData.DiasTardanza * 1m;

                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["SalarioBase"] = salarioBase,
                    ["JornalDiario"] = jornalDiario,
                    ["DiasTrabajados"] = attendanceData.DiasTrabajados,
                    ["DiasAusencia"] = attendanceData.DiasAusencia,
                    ["DiasTardanza"] = attendanceData.DiasTardanza,
                    ["CantidadHijos"] = cantidadHijos,
                    ["TotalDeducibleIPS"] = 0m,
                    ["AniosAntiguedad"] = aniosAntiguedad,
                    ["SueldoMinimo"] = sueldoMinimo,
                    ["ValorHoraOrdinaria"] = valorHoraOrdinaria,
                    ["HorasTardanza"] = horasTardanza,
                    ["CantidadHoras50"] = 0m,
                    ["CantidadHoras100"] = 0m
                };

                var totalDeducibleIPS = 0m;
                var totalHaberes = 0m;
                var totalDescuentos = 0m;

                // PASO A: Haberes Calculados — evalúa fórmulas, acumula IPS deductible
                foreach (var update in calculatedEarnings)
                {
                    var amount = ResolveCalculatedAmount(update, variables);
                    totalHaberes += amount;

                    if (update.IpsDeductible)
                        totalDeducibleIPS += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);
                }

                // PASO B: Absorber novedades manuales pendientes (Haberes y Descuentos)
                foreach (var update in manualUpdates)
                {
                    var key = (employee.Id, update.Id);
                    decimal amount;

                    if (incidentsByEmployeeAndUpdate.TryGetValue(key, out var incidents) && incidents.Count > 0)
                    {
                        amount = incidents.Sum(i => i.Amount);

                        foreach (var incident in incidents)
                            incident.PayrollProcessId = process.Id;
                    }
                    else
                    {
                        amount = 0m;
                    }

                    // Si la novedad manual es INGRESO y su concepto base es deducible de IPS,
                    // debe SUMARSE al acumulador TotalDeducibleIPS
                    if (update.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
                    {
                        totalHaberes += amount;

                        if (update.IpsDeductible)
                            totalDeducibleIPS += amount;
                    }
                    else
                    {
                        totalDescuentos += amount;
                    }

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);
                }

                // Inyectar TotalDeducibleIPS final antes de evaluar descuentos calculados
                variables["TotalDeducibleIPS"] = totalDeducibleIPS;

                // PASO C: Descuentos Calculados — incluye IPS que lee TotalDeducibleIPS
                foreach (var update in calculatedDeductions)
                {
                    var amount = ResolveCalculatedAmount(update, variables);
                    totalDescuentos += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);
                }

                addedCount++;
            }

            await AssignPendingIncidentsAsync(pendingIncidents);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result<int>.Success(addedCount);
        }
        catch (InvalidOperationException exception)
        {
            await transaction.RollbackAsync();
            return Result<int>.Failure(exception.Message, ErrorType.Conflict);
        }
        catch (KeyNotFoundException exception)
        {
            await transaction.RollbackAsync();
            return Result<int>.Failure(exception.Message, ErrorType.Validation);
        }
        catch (FormatException exception)
        {
            await transaction.RollbackAsync();
            return Result<int>.Failure(exception.Message, ErrorType.Validation);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<ListPayrollDetailSummariesWrapperDto>> GetDetailSummariesAsync(int processId, PaginationRequestDto pagination)
    {
        var process = await _context.PayrollProcesses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process is null)
            return Result<ListPayrollDetailSummariesWrapperDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var referenceDate = process.PayDate ?? new DateOnly(process.Year, process.Month, DateTime.DaysInMonth(process.Year, process.Month));

        var details = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Where(d => d.PayrollProcessId == processId)
            .Include(d => d.Employee).ThenInclude(e => e.Branch)
            .Include(d => d.Employee).ThenInclude(e => e.Area)
            .Include(d => d.Employee).ThenInclude(e => e.PositionByScheduleByEmployees).ThenInclude(psbe => psbe.Position)
            .Include(d => d.PayrollUpdate)
            .ToListAsync();

        var allSummaries = details
            .GroupBy(d => d.EmployeeId)
            .Select(g =>
            {
                var firstDetail = g.First();
                var employee = firstDetail.Employee;
                var sueldoBruto = g
                    .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
                    .Sum(d => d.Amount);
                var descuentos = g
                    .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions)
                    .Sum(d => d.Amount);

                var currentPosition = employee.PositionByScheduleByEmployees
                    .Where(psbe =>
                        psbe.StartDate <= referenceDate &&
                        (psbe.EndDate == null || psbe.EndDate >= referenceDate))
                    .OrderByDescending(psbe => psbe.StartDate)
                    .ThenByDescending(psbe => psbe.Id)
                    .Select(psbe => psbe.Position)
                    .FirstOrDefault();

                return new PayrollDetailSummaryResponseDto
                {
                    EmployeeId = employee.Id,
                    FileNumber = employee.FileNumber,
                    FullName = $"{employee.Name} {employee.Lastname}",
                    BranchName = employee.Branch?.Name,
                    AreaName = employee.Area?.Name,
                    PositionName = currentPosition?.Name,
                    SueldoBruto = sueldoBruto,
                    Descuentos = descuentos,
                    SueldoNeto = sueldoBruto - descuentos
                };
            })
            .OrderBy(s => s.FullName)
            .ToList();

        var totalElements = allSummaries.Count;
        var items = allSummaries
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return Result<ListPayrollDetailSummariesWrapperDto>.Success(new ListPayrollDetailSummariesWrapperDto
        {
            Summaries = items,
            Pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements)
        });
    }

    public async Task<Result<List<PayrollConceptSummaryResponseDto>>> GetConceptSummariesAsync(int processId)
    {
        var process = await _context.PayrollProcesses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process is null)
            return Result<List<PayrollConceptSummaryResponseDto>>.Failure(
                PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var details = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Where(d => d.PayrollProcessId == processId)
            .Include(d => d.PayrollUpdate)
            .ToListAsync();

        var earningsConcepts = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings && d.Amount > 0)
            .GroupBy(d => d.PayrollUpdateId)
            .Select(g => new ConceptSummaryItemDto
            {
                ConceptName = g.First().PayrollUpdate.Name,
                TotalAmount = g.Sum(d => d.Amount)
            })
            .OrderBy(c => c.ConceptName)
            .ToList();

        var deductionsConcepts = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions && d.Amount > 0)
            .GroupBy(d => d.PayrollUpdateId)
            .Select(g => new ConceptSummaryItemDto
            {
                ConceptName = g.First().PayrollUpdate.Name,
                TotalAmount = g.Sum(d => d.Amount)
            })
            .OrderBy(c => c.ConceptName)
            .ToList();

        var result = new List<PayrollConceptSummaryResponseDto>
        {
            new() { PayrollType = "Ingresos", Concepts = earningsConcepts },
            new() { PayrollType = "Egresos", Concepts = deductionsConcepts }
        };

        return Result<List<PayrollConceptSummaryResponseDto>>.Success(result);
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

        var statusId = request.PayrollStatusId.HasValue
            ? (PayrollProcess.PayrollStatusEnum)request.PayrollStatusId.Value
            : PayrollProcess.PayrollStatusEnum.Open;

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
        var process = await _context.PayrollProcesses
            .FirstOrDefaultAsync(p => p.Id == id);
        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (IsFinalPayrollStatus(process.PayrollStatusId))
            return Result.Failure(PayrollProcessError.PayrollProcessCannotBeModified, ErrorType.Conflict);

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
            process.PayrollStatusId = (PayrollProcess.PayrollStatusEnum)request.PayrollStatusId.Value;

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeletePayrollProcessAsync(int id)
    {
        var process = await _context.PayrollProcesses.FirstOrDefaultAsync(p => p.Id == id);
        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (IsFinalPayrollStatus(process.PayrollStatusId))
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

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == request.EmployeeId);

        if (employee is null)
            return Result<PayrollManualDetailResponseDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var payrollUpdate = await _context.PayrollUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(payrollUpdate => payrollUpdate.Id == request.PayrollUpdateId);

        if (payrollUpdate is null)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollUpdateError.PayrollUpdateNotFound, ErrorType.NotFound);

        if (payrollUpdate.FormulaTypeId != PayrollUpdate.FormulaTypeEnum.Fixed)
            return Result<PayrollManualDetailResponseDto>.Failure(PayrollManualDetailError.PayrollUpdateMustBeFixed, ErrorType.Validation);

        var roundedAmount = decimal.Round(request.Amount, 0, MidpointRounding.AwayFromZero);
        if (roundedAmount == 0m)
            return Result<PayrollManualDetailResponseDto>.Failure("El monto debe ser mayor a 0", ErrorType.Validation);

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
                Amount = roundedAmount
            };

            _context.PayrollProcessDetails.Add(existingDetail);
        }
        else
        {
            existingDetail.Amount = roundedAmount;
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

        if (detail.PayrollProcess.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
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

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
            return Result<PayrollProcessCalculationResponseDto>.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var payrollUpdates = await _context.PayrollUpdates
            .AsNoTracking()
            .OrderBy(payrollUpdate => payrollUpdate.Id)
            .ToListAsync();

        // Paso A: Haberes Calculados (formula type = Calculated)
        var calculatedEarnings = payrollUpdates
            .Where(pu => pu.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings
                      && pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Calculated)
            .OrderBy(pu => pu.Id)
            .ToList();

        // Paso B: Haberes y Descuentos Manuales (formula type = Fixed)
        var manualUpdates = payrollUpdates
            .Where(pu => pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Fixed)
            .OrderBy(pu => pu.Id)
            .ToList();

        // Paso C: Descuentos Calculados (formula type = Calculated, type = Deductions)
        var calculatedDeductions = payrollUpdates
            .Where(pu => pu.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions
                      && pu.FormulaTypeId == PayrollUpdate.FormulaTypeEnum.Calculated)
            .OrderBy(pu => pu.Id)
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
                var attendanceData = await ResolveAttendanceDataAsync(employee.Id, process.Year, process.Month);
                var cantidadHijos = await ResolveChildrenCountAsync(employee.Id, referenceDate);

                var aniosAntiguedad = process.Year - employee.HireDate.Year;
                if (employee.HireDate.Month > process.Month ||
                    (employee.HireDate.Month == process.Month && employee.HireDate.Day > DateTime.DaysInMonth(process.Year, process.Month)))
                    aniosAntiguedad--;

                var sueldoMinimo = 2899048m;
                var valorHoraOrdinaria = decimal.Round(jornalDiario / 8m, 2, MidpointRounding.AwayFromZero);
                var horasTardanza = attendanceData.DiasTardanza * 1m;

                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["SalarioBase"] = salarioBase,
                    ["JornalDiario"] = jornalDiario,
                    ["DiasTrabajados"] = attendanceData.DiasTrabajados,
                    ["DiasAusencia"] = attendanceData.DiasAusencia,
                    ["DiasTardanza"] = attendanceData.DiasTardanza,
                    ["CantidadHijos"] = cantidadHijos,
                    ["TotalDeducibleIPS"] = 0m,
                    ["AniosAntiguedad"] = aniosAntiguedad,
                    ["SueldoMinimo"] = sueldoMinimo,
                    ["ValorHoraOrdinaria"] = valorHoraOrdinaria,
                    ["HorasTardanza"] = horasTardanza,
                    ["CantidadHoras50"] = 0m,
                    ["CantidadHoras100"] = 0m
                };

                var totalDeducibleIPS = 0m;
                var totalHaberes = 0m;
                var totalDescuentos = 0m;
                var employeeDetails = new List<PayrollProcessDetailCalculationResponseDto>();

                // PASO A: Haberes Calculados — evalúa fórmulas, acumula IPS deductible
                foreach (var update in calculatedEarnings)
                {
                    var amount = ResolveCalculatedAmount(update, variables);

                    totalHaberes += amount;

                    if (update.IpsDeductible)
                        totalDeducibleIPS += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);
                    employeeDetails.Add(CreateDetailResponse(update, amount));
                }

                // PASO B: Absorber conceptos manuales pendientes (Haberes y Descuentos)
                foreach (var update in manualUpdates)
                {
                    var key = (employee.Id, update.Id);
                    decimal amount;

                    if (incidentsByEmployeeAndUpdate.TryGetValue(key, out var incidents) && incidents.Count > 0)
                    {
                        amount = incidents.Sum(i => i.Amount);

                        foreach (var incident in incidents)
                            incident.PayrollProcessId = process.Id;
                    }
                    else
                    {
                        amount = 0m;
                    }

                    if (update.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
                        totalHaberes += amount;
                    else
                        totalDescuentos += amount;

                    if (update.IpsDeductible)
                        totalDeducibleIPS += amount;

                    UpsertDetail(process.Id, employee.Id, update.Id, amount, detailsByKey);
                    employeeDetails.Add(CreateDetailResponse(update, amount));
                }

                // Inyectar TotalDeducibleIPS final antes de evaluar descuentos calculados
                variables["TotalDeducibleIPS"] = totalDeducibleIPS;

                // PASO C: Descuentos Calculados — incluye IPS que lee TotalDeducibleIPS
                foreach (var update in calculatedDeductions)
                {
                    var amount = ResolveCalculatedAmount(update, variables);

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
                    DiasTrabajados = attendanceData.DiasTrabajados,
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

            process.PayrollStatusId = PayrollProcess.PayrollStatusEnum.Closed;

            await AssignPendingIncidentsAsync(pendingIncidents);

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



    public async Task<Result<PayrollCloseResponseDto>> CloseProcessAsync(int payrollProcessId)
    {
        var process = await _context.PayrollProcesses
            .FirstOrDefaultAsync(p => p.Id == payrollProcessId);

        if (process is null)
            return Result<PayrollCloseResponseDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
            return Result<PayrollCloseResponseDto>.Failure("La planilla debe estar en estado 'Abierto' para cerrarse.", ErrorType.Conflict);

        var hasDetails = await _context.PayrollProcessDetails.AnyAsync(d => d.PayrollProcessId == process.Id);

        if (hasDetails)
        {
            var recalculateResult = await CalculateAsync(payrollProcessId);
            if (!recalculateResult.IsSuccess)
                return Result<PayrollCloseResponseDto>.Failure(recalculateResult.ErrorMessage!, ErrorType.Failure);
        }

        process.PayrollStatusId = PayrollProcess.PayrollStatusEnum.Closed;
        process.ClosedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return Result<PayrollCloseResponseDto>.Success(new PayrollCloseResponseDto
        {
            PayrollProcessId = process.Id,
            PayrollProcessName = process.Name,
            StatusMessage = $"Planilla '{process.Name}' cerrada exitosamente."
        });
    }

    public async Task<Result> RemoveEmployeeFromProcessAsync(int payrollProcessId, int employeeId)
    {
        var process = await _context.PayrollProcesses
            .FirstOrDefaultAsync(p => p.Id == payrollProcessId);

        if (process is null)
            return Result.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Open)
            return Result.Failure(PayrollProcessError.PayrollProcessMustBeOpen, ErrorType.Conflict);

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee is null)
            return Result.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var details = await _context.PayrollProcessDetails
            .Where(d => d.PayrollProcessId == payrollProcessId && d.EmployeeId == employeeId)
            .ToListAsync();

        if (details.Count == 0)
            return Result.Failure("El empleado no pertenece a esta planilla.", ErrorType.NotFound);

        _context.PayrollProcessDetails.RemoveRange(details);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<PayrollCloseAndPayResponseDto>> CloseAndPayAsync(int payrollProcessId)
    {
        var process = await _context.PayrollProcesses
            .FirstOrDefaultAsync(p => p.Id == payrollProcessId);

        if (process is null)
            return Result<PayrollCloseAndPayResponseDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        if (process.PayrollStatusId != PayrollProcess.PayrollStatusEnum.Closed)
            return Result<PayrollCloseAndPayResponseDto>.Failure("La planilla debe estar en estado 'Cerrado' para cerrar y pagar.", ErrorType.Conflict);

        var details = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Include(d => d.PayrollUpdate)
            .Where(d => d.PayrollProcessId == process.Id)
            .ToListAsync();

        if (details.Count == 0)
            return Result<PayrollCloseAndPayResponseDto>.Failure("La planilla no tiene detalles calculados para cerrar.", ErrorType.Validation);

        // Calcular totales agrupados por tipo de concepto
        var sueldosJornales = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings
                     && d.Amount > 0)
            .Sum(d => d.Amount);

        var bonificacionFamiliar = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings
                     && (d.PayrollUpdate.Name.Contains("Bonificación", StringComparison.OrdinalIgnoreCase)
                      || d.PayrollUpdate.Name.Contains("Familiar", StringComparison.OrdinalIgnoreCase)
                      || d.PayrollUpdate.Name.Contains("Hijo", StringComparison.OrdinalIgnoreCase))
                     && d.Amount > 0)
            .Sum(d => d.Amount);

        var ipsRetencion = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions
                     && (d.PayrollUpdate.Name.Contains("IPS", StringComparison.OrdinalIgnoreCase)
                      || d.PayrollUpdate.Name.Contains("Aporte", StringComparison.OrdinalIgnoreCase))
                     && d.Amount > 0)
            .Sum(d => d.Amount);

        var totalHaberes = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
            .Sum(d => d.Amount);

        var totalDescuentos = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions)
            .Sum(d => d.Amount);

        var netoPagado = totalHaberes - totalDescuentos;

        // Buscar proceso contable activo para la fecha actual (de pago)
        var today = DateOnly.FromDateTime(DateTime.Now);
        var activeProcess = await _context.AccountantProcesses
            .FirstOrDefaultAsync(ap => !ap.IsClosed && ap.StartDate <= today && ap.EndDate >= today);

        if (activeProcess == null)
            return Result<PayrollCloseAndPayResponseDto>.Failure($"No existe un período contable activo para la fecha actual ({today:dd/MM/yyyy}).", ErrorType.Validation);

        // Buscar cuentas contables por código en el periodo activo
        var accountSueldos = await _context.AccountPlans
            .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Code == "4.2.1.01");

        var accountIps = await _context.AccountPlans
            .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Code == "2.1.2.01");

        var accountCaja = await _context.AccountPlans
            .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Code == "1.1.1.02");

        if (accountSueldos is null)
            return Result<PayrollCloseAndPayResponseDto>.Failure("No se encontró la cuenta contable '4.2.1.01 - Pagos de Salarios (Gasto)'.", ErrorType.Validation);

        if (accountIps is null)
            return Result<PayrollCloseAndPayResponseDto>.Failure("No se encontró la cuenta contable '2.1.2.01 - Retenciones IPS por Pagar'.", ErrorType.Validation);

        if (accountCaja is null)
            return Result<PayrollCloseAndPayResponseDto>.Failure("No se encontró la cuenta contable '1.1.1.02 - Bancos (Cuenta Corriente)'.", ErrorType.Validation);

        var entryDetails = new List<CreateEntryDetailDto>();

        // DEBE: Pagos de Salarios (incluye sueldos + bonificación familiar)
        entryDetails.Add(new CreateEntryDetailDto
        {
            AccountPlanId = accountSueldos.Id,
            Debit = sueldosJornales + (bonificacionFamiliar > 0m ? bonificacionFamiliar : 0m),
            Credit = 0m
        });

        // HABER: Retenciones IPS por Pagar
        if (ipsRetencion > 0m)
        {
            entryDetails.Add(new CreateEntryDetailDto
            {
                AccountPlanId = accountIps.Id,
                Debit = 0m,
                Credit = ipsRetencion
            });
        }

        // HABER: Bancos (Cuenta Corriente)
        entryDetails.Add(new CreateEntryDetailDto
        {
            AccountPlanId = accountCaja.Id,
            Debit = 0m,
            Credit = netoPagado
        });

        var totalDebe = entryDetails.Sum(d => d.Debit);
        var totalHaber = entryDetails.Sum(d => d.Credit);

        // Ajustar diferencia por centésimos si existe
        if (totalDebe != totalHaber)
        {
            var diff = totalDebe - totalHaber;
            entryDetails.Last().Credit += diff;
        }

        var entryResult = await _entryService.CreateAutomaticEntryAsync(
            DateTime.Now,
            $"Pago de nómina: {process.Name} ({process.Month}/{process.Year})",
            ModuleEnum.Salary,
            entryDetails
        );

        if (!entryResult.IsSuccess)
            return Result<PayrollCloseAndPayResponseDto>.Failure(entryResult.ErrorMessage!, ErrorType.Failure);

        // Cambiar estado a Pagado
        process.PayrollStatusId = PayrollProcess.PayrollStatusEnum.Paid;

        process.PayDate = DateOnly.FromDateTime(DateTime.Now);
        process.PaidAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Result<PayrollCloseAndPayResponseDto>.Success(new PayrollCloseAndPayResponseDto
        {
            PayrollProcessId = process.Id,
            PayrollProcessName = process.Name,
            AccountingEntryId = entryResult.Value!.Id,
            TotalSueldosJornales = sueldosJornales,
            TotalBonificacionFamiliar = bonificacionFamiliar,
            TotalIpsRetencion = ipsRetencion,
            TotalNetoPagado = netoPagado,
            StatusMessage = $"Planilla cerrada y pagada exitosamente. Asiento contable #{entryResult.Value.Id} generado en el libro diario."
        });
    }

    public async Task<Result<PayrollEmployeeReceiptDto>> GetEmployeeReceiptAsync(int payrollProcessId, int employeeId)
    {
        var process = await _context.PayrollProcesses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == payrollProcessId);

        if (process is null)
            return Result<PayrollEmployeeReceiptDto>.Failure(PayrollProcessError.PayrollProcessNotFound, ErrorType.NotFound);

        var employee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee is null)
            return Result<PayrollEmployeeReceiptDto>.Failure(EmployeeError.EmployeeNotFound, ErrorType.NotFound);

        var legalPerson = await _context.LegalPersons
            .AsNoTracking()
            .Include(lp => lp.Entity)
            .FirstOrDefaultAsync();

        var position = await _context.PositionByScheduleByEmployees
            .AsNoTracking()
            .Where(psbe => psbe.EmployeeId == employeeId
                       && psbe.StartDate <= (process.PayDate ?? new DateOnly(process.Year, process.Month, DateTime.DaysInMonth(process.Year, process.Month)))
                       && (psbe.EndDate == null || psbe.EndDate >= new DateOnly(process.Year, process.Month, 1)))
            .OrderByDescending(psbe => psbe.StartDate)
            .ThenByDescending(psbe => psbe.Id)
            .Include(psbe => psbe.Position)
            .FirstOrDefaultAsync();

        var details = await _context.PayrollProcessDetails
            .AsNoTracking()
            .Where(d => d.PayrollProcessId == payrollProcessId && d.EmployeeId == employeeId)
            .Include(d => d.PayrollUpdate)
            .ToListAsync();

        if (details.Count == 0)
            return Result<PayrollEmployeeReceiptDto>.Failure("El empleado no tiene detalles calculados en esta planilla.", ErrorType.NotFound);

        var spanishMonths = new Dictionary<int, string>
        {
            { 1, "Enero" }, { 2, "Febrero" }, { 3, "Marzo" }, { 4, "Abril" },
            { 5, "Mayo" }, { 6, "Junio" }, { 7, "Julio" }, { 8, "Agosto" },
            { 9, "Septiembre" }, { 10, "Octubre" }, { 11, "Noviembre" }, { 12, "Diciembre" }
        };

        var period = $"{spanishMonths.GetValueOrDefault(process.Month, process.Month.ToString())}/{process.Year}";
        var payDate = process.PayDate?.ToString("dd/MM/yyyy") ?? "";

        var earnings = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Earnings)
            .Select(d => new ReceiptConceptDto
            {
                ConceptName = d.PayrollUpdate.Name,
                Amount = d.Amount,
                IsIpsDeductible = d.PayrollUpdate.IpsDeductible
            })
            .ToList();

        var deductions = details
            .Where(d => d.PayrollUpdate.PayrollTypeId == PayrollUpdate.PayrollTypeEnum.Deductions)
            .Select(d => new ReceiptConceptDto
            {
                ConceptName = d.PayrollUpdate.Name,
                Amount = d.Amount,
                IsIpsDeductible = d.PayrollUpdate.IpsDeductible
            })
            .ToList();

        var totalEarnings = earnings.Sum(e => e.Amount);
        var totalDeductions = deductions.Sum(d => d.Amount);
        var totalIpsDeductible = details
            .Where(d => d.PayrollUpdate.IpsDeductible)
            .Sum(d => d.Amount);

        return Result<PayrollEmployeeReceiptDto>.Success(new PayrollEmployeeReceiptDto
        {
            CompanyBusinessName = legalPerson?.BussinessName ?? "",
            CompanyCuit = legalPerson?.Entity?.DocumentNumber ?? "",
            CompanyAddress = legalPerson?.Entity?.Address ?? "",
            CompanyPhone = legalPerson?.Entity?.Phone ?? "",
            BranchName = employee.Branch?.Name ?? "",
            BranchAddress = employee.Branch?.Address ?? "",
            EmployeeName = $"{employee.Name} {employee.Lastname}",
            EmployeeDocument = employee.DocumentNumber,
            EmployeeLegajo = employee.FileNumber,
            PositionName = position?.Position.Name ?? "",
            Period = period,
            PayDate = payDate,
            Earnings = earnings,
            Deductions = deductions,
            TotalEarnings = totalEarnings,
            TotalDeductions = totalDeductions,
            TotalIpsDeductible = totalIpsDeductible,
            NetSalary = totalEarnings - totalDeductions
        });
    }

    private decimal ResolveCalculatedAmount(PayrollUpdate payrollUpdate, Dictionary<string, decimal> variables)
    {
        if (string.IsNullOrWhiteSpace(payrollUpdate.Formula))
            throw new InvalidOperationException($"El concepto '{payrollUpdate.Name}' no tiene fórmula definida.");

        var raw = _formulaEvaluator.EvaluateFormula(payrollUpdate.Formula, variables);
        return decimal.Round(raw, 0, MidpointRounding.AwayFromZero);
    }

    private async Task AssignPendingIncidentsAsync(List<ManualConceptIncident> pendingIncidents)
    {
        foreach (var incident in pendingIncidents)
        {
            incident.Status = ManualConceptIncident.ManualConceptStatus.Assigned;
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

    private static bool IsFinalPayrollStatus(PayrollProcess.PayrollStatusEnum status)
    {
        return status == PayrollProcess.PayrollStatusEnum.Closed
            || status == PayrollProcess.PayrollStatusEnum.Paid;
    }

    private void UpsertDetail(
        int payrollProcessId,
        int employeeId,
        int payrollUpdateId,
        decimal amount,
        Dictionary<(int EmployeeId, int PayrollUpdateId), PayrollProcessDetail> detailsByKey)
    {
        var roundedAmount = decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
        if (roundedAmount == 0m)
            return;

        var key = (employeeId, payrollUpdateId);

        if (detailsByKey.TryGetValue(key, out var existingDetail))
        {
            existingDetail.Amount = roundedAmount;
            return;
        }

        var detail = new PayrollProcessDetail
        {
            PayrollProcessId = payrollProcessId,
            EmployeeId = employeeId,
            PayrollUpdateId = payrollUpdateId,
            Amount = roundedAmount
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

    private async Task<(decimal DiasTrabajados, decimal DiasAusencia, decimal DiasTardanza)> ResolveAttendanceDataAsync(int employeeId, int year, int month)
    {
        var records = await _context.DailyAttendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.Date.Year == year && a.Date.Month == month)
            .ToListAsync();

        var ausencias = records.Count(a => a.Status == AttendanceStatus.Absent);
        var tardanzas = records.Count(a => a.Status == AttendanceStatus.Late);
        var diasTrabajados = Math.Max(0, 30 - ausencias);

        return (diasTrabajados, ausencias, tardanzas);
    }

    private async Task<decimal> ResolveChildrenCountAsync(int employeeId, DateOnly referenceDate)
    {
        var adultThreshold = referenceDate.AddYears(-18);

        return await _context.EmployeeRelations
            .AsNoTracking()
            .Where(relation =>
                relation.EmployeeId == employeeId &&
                relation.RelationType == EmployeeRelation.RelationTypeEnum.Child &&
                relation.BirthDate > adultThreshold)
            .CountAsync();
    }
}