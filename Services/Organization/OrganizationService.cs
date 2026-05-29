using BackEnd.DTOs.Responses.Organization;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class OrganizationService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<Result<DepartmentBossResponseDto>> GetDepartmentBossAsync(int branchId, int departmentId)
    {
        var branchDepartment = await _context.BranchDepartments
            .AsNoTracking()
            .Include(x => x.Boss)
                .ThenInclude(x => x.Area)
            .Include(x => x.Boss)
                .ThenInclude(x => x.InmediatlyBoss)
            .Include(x => x.Boss)
                .ThenInclude(x => x.PositionByScheduleByEmployees)
                    .ThenInclude(h => h.Position)
            .Include(x => x.Boss)
                .ThenInclude(x => x.PositionByScheduleByEmployees)
                    .ThenInclude(h => h.Schedule)
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.DepartmentId == departmentId);

        if (branchDepartment == null || branchDepartment.Boss == null)
            return Result<DepartmentBossResponseDto>.Failure("No se encontró jefe para ese departamento en esa sucursal.", ErrorType.NotFound);

        var boss = branchDepartment.Boss;
        var latestHistory = boss.PositionByScheduleByEmployees
            .OrderByDescending(h => h.StartDate)
            .ThenByDescending(h => h.Id)
            .FirstOrDefault();

        return Result<DepartmentBossResponseDto>.Success(new DepartmentBossResponseDto
        {
            BranchId = branchDepartment.BranchId,
            DepartmentId = branchDepartment.DepartmentId,
            BossId = boss.Id,
            BossFileNumber = boss.FileNumber,
            BossName = boss.Name,
            BossLastname = boss.Lastname,
            AreaName = boss.Area.Name,
            PositionName = latestHistory?.Position.Name,
            ScheduleName = ToScheduleLabel(latestHistory?.Schedule.ScheduleType)
        });
    }

    public async Task<Result<OrgChartResponseDto>> GetOrgChartAsync(int employeeId, int depth = 3)
    {
        var employees = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Area)
            .Include(e => e.InmediatlyBoss)
            .Include(e => e.PositionByScheduleByEmployees)
                .ThenInclude(h => h.Position)
            .Include(e => e.PositionByScheduleByEmployees)
                .ThenInclude(h => h.Schedule)
            .ToListAsync();

        var root = employees.FirstOrDefault(e => e.Id == employeeId);
        if (root == null)
            return Result<OrgChartResponseDto>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        var chart = BuildNode(root, employees, depth);
        return Result<OrgChartResponseDto>.Success(chart);
    }

    private static OrgChartResponseDto BuildNode(Employee employee, IReadOnlyList<Employee> employees, int depth)
    {
        var latestHistory = employee.PositionByScheduleByEmployees
            .OrderByDescending(h => h.StartDate)
            .ThenByDescending(h => h.Id)
            .FirstOrDefault();

        var node = new OrgChartResponseDto
        {
            EmployeeId = employee.Id,
            FileNumber = employee.FileNumber,
            Name = employee.Name,
            Lastname = employee.Lastname,
            FullName = $"{employee.Name} {employee.Lastname}",
            InmediatlyBossId = employee.InmediatlyBossId,
            InmediatlyBossName = employee.InmediatlyBoss == null ? null : $"{employee.InmediatlyBoss.Name} {employee.InmediatlyBoss.Lastname}",
            AreaName = employee.Area.Name,
            PositionName = latestHistory?.Position.Name,
            ScheduleName = ToScheduleLabel(latestHistory?.Schedule.ScheduleType)
        };

        if (depth <= 0)
            return node;

        var reports = employees
            .Where(e => e.InmediatlyBossId == employee.Id)
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Lastname)
            .Select(e => BuildNode(e, employees, depth - 1))
            .ToList();

        node.Reports = reports;
        return node;
    }

    private static string? ToScheduleLabel(ScheduleTypeEnum? scheduleType)
    {
        return scheduleType switch
        {
            ScheduleTypeEnum.Morning => "Turno Mañana",
            ScheduleTypeEnum.Afternoon => "Turno Tarde",
            ScheduleTypeEnum.Night => "Turno Noche",
            ScheduleTypeEnum.FullTime => "Jornada Completa",
            ScheduleTypeEnum.PartTime => "Medio Tiempo",
            _ => null
        };
    }
}