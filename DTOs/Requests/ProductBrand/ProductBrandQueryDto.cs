using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.ProductBrand;

public class ProductBrandQueryDto : PaginationRequestDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}
