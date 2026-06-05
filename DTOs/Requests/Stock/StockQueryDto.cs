using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.Stock;

public class StockQueryDto : PaginationRequestDto
{
    public int? ProductId { get; set; }
    public int? BranchId { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public string? Search { get; set; }
    public bool? BelowMinimum { get; set; }
}
