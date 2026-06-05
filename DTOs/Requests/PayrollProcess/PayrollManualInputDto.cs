namespace BackEnd.DTOs.Requests.PayrollProcess;

public class PayrollManualInputDto
{
    public int EmployeeId { get; set; }
    public int PayrollUpdateId { get; set; }
    public decimal Amount { get; set; }
}