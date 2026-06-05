using BackEnd.Utils;

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

    public CheckTypeEnum Type { get; set; }

    public string IssuingBank { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public decimal Amount { get; set; }


    public CheckStatusEnum Status { get; set; }
}

public class CheckWrapperDto
{
    public CheckResponseDto Check { get; set; } = null!;
}

public class ListChecksWrapperDto
{
    public List<CheckResponseDto> Checks { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}

