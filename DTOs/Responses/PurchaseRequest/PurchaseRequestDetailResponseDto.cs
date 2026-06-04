namespace BackEnd.DTOs.Responses.PurchaseRequest;

public class PurchaseRequestDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantityRequested { get; set; }
}
