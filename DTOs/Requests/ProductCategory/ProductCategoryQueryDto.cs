using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.ProductCategory;

public class ProductCategoryQueryDto : PaginationRequestDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}
