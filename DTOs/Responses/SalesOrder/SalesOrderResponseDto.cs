using System;

namespace BackEnd.DTOs.Responses.SalesOrder;

public class SalesOrderResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int UserId { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public int StateId { get; set; }
}

public class SalesOrderWrapperDto
{
    public SalesOrderResponseDto SalesOrder { get; set; } = null!;
}
