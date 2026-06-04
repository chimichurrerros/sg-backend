namespace BackEnd.DTOs.Responses.Supplier;

using BackEnd.Utils;

public class SupplierResponseDto
{
    public int Id { get; set; }
    public string Ruc { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? FantasyName { get; set; }
    public List<SupplierCategoryResponseDto> SupplierCategories { get; set; } = [];
}

public class SupplierWrapperDto
{
    public SupplierResponseDto Supplier { get; set; } = null!;
}

public class ListSuppliersWrapperDto
{
    public List<SupplierResponseDto> Suppliers { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
