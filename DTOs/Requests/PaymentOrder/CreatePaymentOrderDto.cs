namespace BackEnd.DTOs.Requests.PaymentOrder;

public class CreatePaymentOrderDto
{
    public int PurchaseOrderForSupplierId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public List<PaymentMethodLineDto> Methods { get; set; } = new();
}
