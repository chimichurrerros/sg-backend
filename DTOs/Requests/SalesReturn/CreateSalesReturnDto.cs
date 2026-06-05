namespace BackEnd.DTOs.Requests.SalesReturn;

public class CreateSalesReturnDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreateSalesReturnDto
{
    public int BillId { get; set; }
    public string? Number { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<CreateSalesReturnDetailDto> Details { get; set; } = new();
}
