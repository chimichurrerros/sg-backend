namespace BackEnd.DTOs.Responses.PayrollProcess;

public class EligibleEmployeeResponseDto
{
    public int Id { get; set; }
    public string FileNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? BranchName { get; set; }
    public string? AreaName { get; set; }
    public string? PositionName { get; set; }
}
