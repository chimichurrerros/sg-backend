namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollManualDetailResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = null!;
    public string ConceptName { get; set; } = null!;
    public string PayrollTypeName { get; set; } = null!;
    public decimal Amount { get; set; }
}