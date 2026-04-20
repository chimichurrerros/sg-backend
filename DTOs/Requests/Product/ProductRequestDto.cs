namespace BackEnd.DTOs.Requests.Product;

public partial class ProductRequestDto
{
    public int ProductCategoryId { get; set; }

    public int ProductBrandId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal Cost { get; set; }

    public decimal MinimumStock { get; set; }
}
