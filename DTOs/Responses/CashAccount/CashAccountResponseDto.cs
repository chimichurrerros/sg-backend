using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.CashAccount;

public class CashAccountResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
}

public class CashAccountWrapperDto
{
    public CashAccountResponseDto CashAccount { get; set; } = null!;
}

public class ListCashAccountsWrapperDto
{
    public List<CashAccountResponseDto> CashAccounts { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
