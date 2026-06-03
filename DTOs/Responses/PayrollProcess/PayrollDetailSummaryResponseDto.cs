namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollDetailSummaryResponseDto
{
    public int EmployeeId { get; set; }
    public string FileNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? BranchName { get; set; }
    public string? AreaName { get; set; }
    public string? PositionName { get; set; }
    public decimal SueldoBruto { get; set; }
    public decimal Descuentos { get; set; }
    public decimal SueldoNeto { get; set; }
}
