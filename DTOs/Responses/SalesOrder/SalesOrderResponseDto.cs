using BackEnd.Models;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.SalesOrder;

public class SalesOrderResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public SalesOrderStateEnum SalesOrderState { get; set; }
    public List<SalesOrderDetailResponseDto> Details { get; set; } = new();
    public List<BillResponseDto> Bills { get; set; } = new();
}

public class SalesOrderDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
}

public class SalesOrderWrapperDto
{
    public SalesOrderResponseDto SalesOrder { get; set; } = null!;
}

public class ListSalesOrdersWrapperDto
{
    public List<SalesOrderResponseDto> SalesOrders { get; set; } = new();
    public Pagination? Pagination { get; set; }
}
