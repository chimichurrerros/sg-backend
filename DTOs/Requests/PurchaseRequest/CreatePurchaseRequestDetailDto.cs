namespace BackEnd.DTOs.Requests.PurchaseRequest;

public class CreatePurchaseRequestDetailDto
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public decimal QuantityRequested { get; set; }
}
