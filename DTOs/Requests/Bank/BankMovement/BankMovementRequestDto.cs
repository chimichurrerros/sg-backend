namespace BackEnd.DTOs.Requests.Bank.BankMovement;

public class BankMovementRequestDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Date { get; set; }
}