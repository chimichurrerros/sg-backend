namespace BackEnd.DTOs.Requests.Accounts;
using BackEnd.Models; 

public class UpdateAccountRequestDto
{


    public AccountTypeEnum AccountType { get; set; }

    public string Name { get; set; } = null!;

    
}