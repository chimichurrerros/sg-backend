namespace BackEnd.DTOs.Responses.Checks;     
using BackEnd.Models;

public class CheckResponseDto
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime EmisionDate { get; set; }

    public DateOnly AvailabilityDate { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public DateOnly? MaturityDate { get; set; }
    
    public CheckType Type { get; set; }

    public string IssuingBank { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public decimal Amount { get; set; }

    public CheckStatus Status { get; set; }
}

