namespace BackEnd.DTOs.Responses.PayrollProcess;

public class PayrollCloseAndPayResponseDto
{
    public int PayrollProcessId { get; set; }
    public string PayrollProcessName { get; set; } = null!;
    public int AccountingEntryId { get; set; }
    public decimal TotalSueldosJornales { get; set; }
    public decimal TotalBonificacionFamiliar { get; set; }
    public decimal TotalIpsRetencion { get; set; }
    public decimal TotalNetoPagado { get; set; }
    public string StatusMessage { get; set; } = null!;
}
