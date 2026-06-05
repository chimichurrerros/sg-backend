namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollProcessResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int ProcessTypeId { get; set; }
    public string ProcessTypeName { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? PayDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public int PayrollStatusId { get; set; }
    public string PayrollStatusName { get; set; } = null!;
}
