namespace BackEnd.DTOs.Requests.PayrollProcess;

public class PayrollProcessUpdateDto
{
    public string Name { get; set; } = null!;
    public int ProcessTypeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? PayDate { get; set; }
    public int? PayrollStatusId { get; set; }
}
