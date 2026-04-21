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