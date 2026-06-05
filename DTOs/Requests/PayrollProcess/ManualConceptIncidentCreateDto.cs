namespace BackEnd.DTOs.Requests.PayrollProcess;

public class ManualConceptIncidentCreateDto
{
    public int EmployeeId { get; set; }
    public int PayrollUpdateId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly OccurrenceDate { get; set; }
}