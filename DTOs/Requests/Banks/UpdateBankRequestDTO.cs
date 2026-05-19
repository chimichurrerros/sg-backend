using BackEnd.Models;
namespace BackEnd.DTOs.Requests.Bank;

public class UpdateBankRequestDto
{
    public string? Name { get; set; }
    public string? AccountNumber { get; set; }
    public BankMovementTypeEnum? AccountType { get; set; }
    public string? Ruc { get; set; }

}