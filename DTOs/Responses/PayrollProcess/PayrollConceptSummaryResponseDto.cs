namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollConceptSummaryResponseDto
{
    public string PayrollType { get; set; } = null!;
    public List<ConceptSummaryItemDto> Concepts { get; set; } = [];
}

public class ConceptSummaryItemDto
{
    public string ConceptName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}
