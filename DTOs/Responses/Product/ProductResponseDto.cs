using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Product;


public class ProductResponseDto
{
    public int Id { get; set; }
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = null!;
    public int ProductBrandId { get; set; }
    public string ProductBrandName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public decimal TaxRate { get; set; }
    public decimal MinimumStock { get; set; }
    public bool? IsDeleted {get; set;}
}

public class ProductWrapperDto
{
    public ProductResponseDto Product { get; set; } = null!;
}

// Crear todo los ListWrapper con paginacion
public class ListProductsWrapperDto
{
    public List<ProductResponseDto> Products { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}

public class ProductStockResponseDto
{
    public int Id { get; set; }
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = null!;
    public int ProductBrandId { get; set; }
    public string ProductBrandName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public decimal MinimumStock { get; set; }
    // Stock field
    public decimal Quantity { get; set; }
}
public class ProductStockWrapperDto
{
    public ProductStockResponseDto ProductStock { get; set; } = null!;
}
public class ListProductsStockWrapperDto
{
    public List<ProductStockResponseDto> ProductsStock { get; set; } = [];
}
