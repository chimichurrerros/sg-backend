namespace BackEnd.DTOs.Responses.PayrollProcess;

public class ManualConceptIncidentResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = null!;
    public int PayrollUpdateId { get; set; }
    public string ConceptName { get; set; } = null!;
    public string PayrollTypeName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly OccurrenceDate { get; set; }
    public string StatusName { get; set; } = null!;
    public int? PayrollProcessId { get; set; }
}