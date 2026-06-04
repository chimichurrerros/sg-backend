using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Employee;

public class PositionByScheduleByEmployeeDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = EmployeeError.EmployeeNotFound)]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = EmployeeError.InvalidPosition)]
    public int PositionId { get; set; }

    [Required(ErrorMessage = EmployeeError.InvalidSchedule)]
    public int ScheduleId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = EmployeeError.BasicSalaryMustBeGreaterThanZero)]
    public decimal BasicSalary { get; set; }

    [Required(ErrorMessage = EmployeeError.PositionStartDateRequired)]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}