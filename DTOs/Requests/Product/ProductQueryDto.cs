using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.Product;

public class ProductQueryDto : PaginationRequestDto
{
    public string? Name { get; set; }
    public string? BrandName { get; set; }
    public int? ProductBrandId { get; set; }
    public string? CategoryName { get; set; }
    public int? ProductCategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
}
