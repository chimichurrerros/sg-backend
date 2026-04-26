using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public int EntityId { get; set; }
    public string FileNumber { get; set; } = null!;
    public int AreaId { get; set; }
    public int? InmediatlyBossId { get; set; }
    public DateOnly HireDate { get; set; }

    // Relacionales
    public virtual object? Area { get; set; }
    public virtual object? Entity { get; set; } // Representa a la PhysicalPerson devuelta por la BD
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
