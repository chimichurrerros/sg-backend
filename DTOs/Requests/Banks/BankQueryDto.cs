using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Models;
using System;

namespace BackEnd.DTOs.Requests.Bank;

public class BankQueryDto : PaginationRequestDto
{
    public string? Name { get; set; } // NOMBRE DEL BANCO
    public string? Representative { get; set; } // REPRESENTANTE (Account.Name)
    public string? Ruc { get; set; } // RUC DEL BANCO
    public AccountTypeEnum? Type { get; set; } // TIPO (Account.AccountType)
    public string? AccountNumber { get; set; } // NRO CUENTA (Account.AccountNumber)
    public bool? IsActive { get; set; } // SITUACION (Bank.IsActive)
}
