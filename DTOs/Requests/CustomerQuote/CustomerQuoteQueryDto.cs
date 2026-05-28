using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.CustomerQuote;

public class CustomerQuoteQueryDto : PaginationRequestDto
{
    public int? Id { get; set; } // NRO DE PRESUPUESTO
    public DateTime? Date { get; set; } // FECHA DE CREACION
    public DateTime? ExpirationDate { get; set; } // FECHA DE EXPIRACION
    public string? CustomerName { get; set; } // CLIENTE
    public int? CustomerId { get; set; } // CLIENTE ID
}
