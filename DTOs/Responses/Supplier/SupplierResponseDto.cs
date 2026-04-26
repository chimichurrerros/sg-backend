namespace BackEnd.DTOs.Responses.Supplier;

using BackEnd.Utils;

public class SupplierResponseDto
{
    public int Id { get; set; }
    public int EntityId { get; set; }
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
