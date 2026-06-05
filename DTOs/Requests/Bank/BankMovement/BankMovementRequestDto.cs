namespace BackEnd.DTOs.Requests.Bank.BankMovement;

using BackEnd.DTOs.Requests.Checks;
using BackEnd.Models;
public class BankMovementRequestDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }

    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public BankMovementTypeEnum MovementType { get; set; }
    public CreateCheckRequestDto? CheckDetails { get; set; }
}