using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.ProductCategory;

public class ProductCategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class ProductCategoryWrapperDto
{
    public ProductCategoryResponseDto ProductCategory { get; set; } = null!;
}

public class ListProductCategoriesWrapperDto
{
    public List<ProductCategoryResponseDto> ProductCategories { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
