namespace BackEnd.DTOs.Requests.PaymentOrder;

public class ProcessPaymentOrderDto
{
    public int PaymentOrderId { get; set; }
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
}
