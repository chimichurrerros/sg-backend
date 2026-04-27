namespace BackEnd.DTOs.Requests.Accounts;
using BackEnd.Models; 

public class UpdateAccountRequestDto
{


    public AccountTypeEnum AccountType { get; set; }

    public string Name { get; set; } = null!;

    public decimal CurrentBalance { get; set; }

    public decimal AvailableBalance { get; set; }
}