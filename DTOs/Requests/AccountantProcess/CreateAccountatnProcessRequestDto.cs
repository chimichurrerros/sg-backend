using BackEnd.Models;

namespace BackEnd.DTOs.Requests.AccountantProcess;

public class CreateAccountantProcessRequestDto
{

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsClosed { get; set; }
}