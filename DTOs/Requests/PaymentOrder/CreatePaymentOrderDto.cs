namespace BackEnd.DTOs.Requests.PaymentOrder;

public class CreatePaymentOrderDto
{
    public int PurchaseOrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
