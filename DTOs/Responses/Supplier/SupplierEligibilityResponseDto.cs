namespace BackEnd.DTOs.Responses.Supplier;

public class EligibleSupplierDto
{
    public int SupplierId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? FantasyName { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();
}

public class EligibleSuppliersWrapperDto
{
    public List<EligibleSupplierDto> EligibleSuppliers { get; set; } = new();
}
