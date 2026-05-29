using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.Service;

public class ServiceQueryDto : PaginationRequestDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
}
