using BackEnd.Models;
namespace BackEnd.DTOs.Requests.Bank;

public class UpdateBankRequestDto
{
    public string? Name { get; set; }
    public string? Ruc { get; set; }

}