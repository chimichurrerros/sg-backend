namespace BackEnd.DTOs.Responses.Organization;

public class OrgChartResponseDto
{
    public int EmployeeId { get; set; }
    public string FileNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int? InmediatlyBossId { get; set; }
    public string? InmediatlyBossName { get; set; }
    public string? AreaName { get; set; }
    public string? PositionName { get; set; }
    public string? ScheduleName { get; set; }
    public List<OrgChartResponseDto> Reports { get; set; } = [];
}