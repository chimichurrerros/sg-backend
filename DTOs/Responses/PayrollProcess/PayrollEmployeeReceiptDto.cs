namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollEmployeeReceiptDto
{
    public string CompanyBusinessName { get; set; } = null!;
    public string CompanyCuit { get; set; } = null!;
    public string CompanyAddress { get; set; } = null!;
    public string CompanyPhone { get; set; } = null!;

    public string BranchName { get; set; } = null!;
    public string BranchAddress { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;
    public string EmployeeDocument { get; set; } = null!;
    public string EmployeeLegajo { get; set; } = null!;
    public string PositionName { get; set; } = null!;

    public string Period { get; set; } = null!;
    public string PayDate { get; set; } = null!;

    public List<ReceiptConceptDto> Earnings { get; set; } = [];
    public List<ReceiptConceptDto> Deductions { get; set; } = [];

    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalIpsDeductible { get; set; }
    public decimal NetSalary { get; set; }
}

public class ReceiptConceptDto
{
    public string ConceptName { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsIpsDeductible { get; set; }
}
