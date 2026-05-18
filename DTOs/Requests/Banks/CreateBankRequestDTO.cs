using BackEnd.Models;
namespace BackEnd.DTOs.Requests.Bank;

public class BankRequestDto
{
    public required string Name { get; set; } = null!;
    public required string AccountNumber { get; set; }
    public required BankMovementTypeEnum AccountType { get; set; }
    public required string Ruc { get; set; }

}