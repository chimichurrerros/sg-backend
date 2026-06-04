using BackEnd.Models;

namespace BackEnd.DTOs.Requests.AccountantProcess;

public class UpdateAccountantProcessRequestDto
{

    public string Name { get; set; } = null!;

    public bool IsClosed { get; set; }
}