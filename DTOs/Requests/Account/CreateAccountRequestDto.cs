namespace BackEnd.DTOs.Requests.Accounts;
using BackEnd.Models;

public class CreateAccountRequestDto
{
    public AccountTypeEnum AccountType { get; set; }

    public int? BankId { get; set; }

    public string Name { get; set; } = null!;

    public decimal CurrentBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    public string AccountNumber { get; set; } = null!;
}