namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollProcessCalculationResponseDto
{
    public int PayrollProcessId { get; set; }
    public string PayrollProcessName { get; set; } = null!;
    public int EmployeesProcessed { get; set; }
    public decimal TotalHaberes { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalNeto { get; set; }
    public List<PayrollEmployeeCalculationResponseDto> Employees { get; set; } = [];
}

public class PayrollEmployeeCalculationResponseDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public decimal SalarioBase { get; set; }
    public decimal JornalDiario { get; set; }
    public decimal DiasTrabajados { get; set; }
    public decimal CantidadHijos { get; set; }
    public decimal TotalDeducibleIPS { get; set; }
    public decimal TotalHaberes { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalNeto { get; set; }
    public List<PayrollProcessDetailCalculationResponseDto> Details { get; set; } = [];
}

public class PayrollProcessDetailCalculationResponseDto
{
    public int PayrollUpdateId { get; set; }
    public string PayrollUpdateName { get; set; } = null!;
    public int PayrollTypeId { get; set; }
    public int FormulaTypeId { get; set; }
    public decimal Amount { get; set; }
}