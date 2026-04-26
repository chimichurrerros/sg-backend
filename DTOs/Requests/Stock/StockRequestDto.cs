namespace BackEnd.DTOs.Requests.Stock;

public class StockRequestDto
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public decimal Quantity { get; set; }
}
