namespace BackEnd.DTOs.Responses.Organization;

public class OrganizationEmployeeBossResponseDto
{
    public int EmployeeId { get; set; }
    public string FileNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AreaName { get; set; }
    public string? PositionName { get; set; }
    public string? ScheduleName { get; set; }
}