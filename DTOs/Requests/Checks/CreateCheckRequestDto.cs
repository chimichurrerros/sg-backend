
namespace BackEnd.DTOs.Requests.Checks;
using BackEnd.Models;
public class CreateCheckRequestDto
{
    public string Number { get; set; } = null!;

    public DateTime EmisionDate { get; set; }

    public DateOnly? AvailabilityDate { get; set; }

    public CheckType Type { get; set; }

    public string IssuingBank { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public decimal Amount { get; set; }
}