using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FileNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public BackEnd.Models.Employee.GenderEnum Gender { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int AreaId { get; set; }
    public string AreaName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? InmediatlyBossId { get; set; }
    public DateOnly HireDate { get; set; }
    public BackEnd.Models.Employee.MaritalStatusEnum MaritalStatus { get; set; }

    // From PositionByScheduleByEmployee (latest)
    public decimal? BaseSalary { get; set; }
    public DateOnly? PositionStartDate { get; set; }
    public int? PositionId { get; set; }
    public string? PositionName { get; set; }
    public int? ScheduleId { get; set; }
    public string? ScheduleName { get; set; }

}

public class EmployeeWrapperDto
{
    public EmployeeResponseDto Employee { get; set; } = null!;
}

public class ListEmployeesWrapperDto
{
    public List<EmployeeResponseDto> Employees { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
