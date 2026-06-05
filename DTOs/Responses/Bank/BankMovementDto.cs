using BackEnd.Models;

namespace BackEnd.DTOs.Responses.Bank;

public class BankMovementDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }
    public BankMovementTypeEnum MovementType { get; set; }
}
