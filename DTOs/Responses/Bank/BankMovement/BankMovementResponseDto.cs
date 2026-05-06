namespace BackEnd.DTOs.Responses.Bank.BankMovement;

public class BankMovementResponseDto
{
    public int Id { get; set; }
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Date { get; set; }
}