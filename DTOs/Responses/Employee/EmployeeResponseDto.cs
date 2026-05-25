using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public int EntityId { get; set; }
    public string FileNumber { get; set; } = null!;
    public int AreaId { get; set; }
    public int? BranchId { get; set; }
    public int? InmediatlyBossId { get; set; }
    public DateOnly HireDate { get; set; }
    public BackEnd.Models.Employee.MaritalStatusEnum MaritalStatus { get; set; }

    // From PositionByScheduleByEmployee (latest)
    public decimal? BaseSalary { get; set; }
    public DateOnly? PositionStartDate { get; set; }

    // Relacionales
    public virtual object? Area { get; set; }
    public virtual object? Entity { get; set; } // Representa a la PhysicalPerson devuelta por la BD
    public virtual object? Branch { get; set; }
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
