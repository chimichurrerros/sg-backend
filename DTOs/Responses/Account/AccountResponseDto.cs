using BackEnd.Models; 
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Accounts;

public class AccountResponseDto
{
    public int Id { get; set; }
    public AccountTypeEnum AccountType { get; set; }
    public int? BankId { get; set; }
    public string Name { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string AccountNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class AccountWrapperDto
{
    public AccountResponseDto Account { get; set; } = null!;
}

public class ListAccountsWrapperDto
{
    public List<AccountResponseDto> Accounts { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}