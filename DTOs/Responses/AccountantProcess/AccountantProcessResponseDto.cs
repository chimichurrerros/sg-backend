using BackEnd.Models;
using BackEnd.Utils;
namespace BackEnd.DTOs.Responses.AccountantProcess;

public class AccountantProcessResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsClosed { get; set; }
}

public class AccountantProcessWrapperDto
{
    public AccountantProcessResponseDto AccountantProcess { get; set; } = null!;
}

public class ListAccountantProcessesWrapperDto
{
    public List<AccountantProcessResponseDto> AccountantProcesses { get; set; } = [];

    public Pagination? Pagination { get; set; }
}