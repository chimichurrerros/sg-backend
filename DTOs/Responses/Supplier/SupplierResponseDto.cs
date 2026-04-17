namespace BackEnd.DTOs.Responses.Supplier;

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
}
