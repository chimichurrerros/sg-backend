
namespace BackEnd.DTOs.Requests.Checks;

using BackEnd.Models;
public class CreateCheckRequestDto
{
    public int AccountId { get; set; }
    public string Number { get; set; } = null!;

    public DateTime EmisionDate { get; set; }

    public DateOnly? AvailabilityDate { get; set; }
    public string IssuingBank { get; set; } = null!;

    public CheckTypeEnum Type { get; set; }

    public string Receiver { get; set; } = null!;

}