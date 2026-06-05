namespace BackEnd.DTOs.Requests.PurchaseRequest;

public class CreatePurchaseRequestDto
{
    public int BranchId { get; set; }
    public string? Observation { get; set; }
    public List<CreatePurchaseRequestDetailDto> Details { get; set; } = new();
    public List<int> SupplierIds { get; set; } = new();
}
