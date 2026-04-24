using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.ProductBrand;

public class ProductBrandResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class ProductBrandWrapperDto
{
    public ProductBrandResponseDto ProductBrand { get; set; } = null!;
}

public class ListProductBrandsWrapperDto
{
    public List<ProductBrandResponseDto> ProductBrands { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
