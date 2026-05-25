namespace BackEnd.DTOs.Responses.Supplier;

using BackEnd.Utils;
using BackEnd.DTOs.Responses.ProductCategory;
public class SupplierCategoryResponseDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int ProductCategoryId { get; set; }
    public ProductCategoryResponseDto ProductCategory { get; set; } = null!;
}

public class SupplierCategoryWrapperDto
{
    public SupplierCategoryResponseDto SupplierCategory { get; set; } = null!;
}

// Crear todo los ListWrapper con paginacion
public class ListSupplierCategoryWrapperDto
{
    public List<SupplierCategoryResponseDto> SupplierCategories { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
