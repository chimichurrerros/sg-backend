namespace BackEnd.DTOs.Responses.Organization;

public class DepartmentBossResponseDto
{
    public int BranchId { get; set; }
    public int DepartmentId { get; set; }
    public int? BossId { get; set; }
    public string? BossFileNumber { get; set; }
    public string? BossName { get; set; }
    public string? BossLastname { get; set; }
    public string? AreaName { get; set; }
    public string? PositionName { get; set; }
    public string? ScheduleName { get; set; }
}