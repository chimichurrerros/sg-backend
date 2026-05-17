namespace BackEnd.DTOs.Requests.CashAccount;

public partial class CashAccountRequestDto
{
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public decimal InitialBalance { get; set; }
}
