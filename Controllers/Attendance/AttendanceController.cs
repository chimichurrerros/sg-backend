using BackEnd.DTOs.Requests.Attendance;
using BackEnd.DTOs.Responses.Attendance;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers.Attendance;

[Route("api/attendance")]
[ApiController]
[Authorize]
public class AttendanceController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpPost]
    public async Task<ActionResult<AttendanceResponseDto>> Create(CreateAttendanceRequestDto request)
    {
        if (!Enum.IsDefined(typeof(AttendanceStatus), request.Status))
            return BadRequest(new { message = $"El estado '{request.Status}' no es válido." });

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

        if (employee is null)
            return NotFound(new { message = "No se encontró el empleado solicitado." });

        var existing = await _context.DailyAttendances
            .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.Date == request.Date);

        if (existing is not null)
        {
            existing.Status = (AttendanceStatus)request.Status;
        }
        else
        {
            existing = new DailyAttendance
            {
                EmployeeId = request.EmployeeId,
                Date = request.Date,
                Status = (AttendanceStatus)request.Status
            };
            _context.DailyAttendances.Add(existing);
        }

        await _context.SaveChangesAsync();

        return Ok(new AttendanceResponseDto
        {
            Id = existing.Id,
            EmployeeId = existing.EmployeeId,
            EmployeeFullName = $"{employee.Name} {employee.Lastname}",
            Date = existing.Date.ToString("yyyy-MM-dd"),
            Status = (int)existing.Status,
            StatusName = existing.Status.ToString()
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<AttendanceResponseDto>>> GetList(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int employeeId,
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        IQueryable<DailyAttendance> query = _context.DailyAttendances
            .AsNoTracking()
            .Include(a => a.Employee);

        if (fromDate.HasValue && toDate.HasValue)
        {
            query = query.Where(a => a.Date >= fromDate.Value && a.Date <= toDate.Value);
        }
        else if (year.HasValue && month.HasValue)
        {
            query = query.Where(a => a.Date.Year == year.Value && a.Date.Month == month.Value);
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(a => a.Date.Year == today.Year && a.Date.Month == today.Month);
        }

        if (employeeId > 0)
            query = query.Where(a => a.EmployeeId == employeeId);

        var result = await query
            .OrderBy(a => a.Employee.Name)
            .ThenBy(a => a.Employee.Lastname)
            .ThenBy(a => a.Date)
            .Select(a => new AttendanceResponseDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = a.Employee.Name + " " + a.Employee.Lastname,
                Date = a.Date.ToString("yyyy-MM-dd"),
                Status = (int)a.Status,
                StatusName = a.Status.ToString()
            })
            .ToListAsync();

        return Ok(result);
    }
}
