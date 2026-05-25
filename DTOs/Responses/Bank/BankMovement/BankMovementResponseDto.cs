using BackEnd.Utils;
using BackEnd.Models;
namespace BackEnd.DTOs.Responses.Bank.BankMovement;

public class BankMovementResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int AccountId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Date { get; set; }
    public BankMovementTypeEnum MovementType { get; set; }
}

public class BankMovementWrapperDto
{
    public BankMovementResponseDto BankMovement { get; set; } = null!;
}

public class ListBankMovementsWrapperDto
{
    public List<BankMovementResponseDto> BankMovements { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}