using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Employee;

public class UpdateEmployeePositionHistoryRequestDto
{
    [Required(ErrorMessage = EmployeeError.InvalidPosition)]
    public int PositionId { get; set; }

    [Required(ErrorMessage = EmployeeError.InvalidSchedule)]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = EmployeeError.BasicSalaryRequired)]
    public decimal BasicSalary { get; set; }

    [Required(ErrorMessage = EmployeeError.PositionStartDateRequired)]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
