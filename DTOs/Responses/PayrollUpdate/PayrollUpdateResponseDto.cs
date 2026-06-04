namespace BackEnd.DTOs.Responses.PayrollUpdate;

public class PayrollUpdateResponseDto
{
    public int Id { get; set; }
    public int PayrollTypeId { get; set; }
    public string PayrollTypeName { get; set; } = null!;
    public int FormulaTypeId { get; set; }
    public string FormulaTypeName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Formula { get; set; }
    public bool IpsDeductible { get; set; }
}