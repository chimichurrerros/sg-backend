using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Stock;

public class StockResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class StockWrapperDto
{
    public StockResponseDto Stock { get; set; } = null!;
}

public class ListStocksWrapperDto
{
    public List<StockResponseDto> Stocks { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
