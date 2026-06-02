namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollCloseResponseDto
{
    public int PayrollProcessId { get; set; }
    public string PayrollProcessName { get; set; } = null!;
    public string StatusMessage { get; set; } = null!;
}
