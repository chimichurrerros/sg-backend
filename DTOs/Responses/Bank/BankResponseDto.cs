using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Bank;

public class BankResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Ruc { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<BankAccountResponseDto> Accounts { get; set; } = [];
}

public class BankAccountResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
}

public class BankWrapperDto
{
    public BankResponseDto Bank { get; set; } = null!;
}

public class ListBanksWrapperDto
{
    public List<BankResponseDto> Banks { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
