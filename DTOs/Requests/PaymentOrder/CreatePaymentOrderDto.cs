using BackEnd.DTOs.Requests.Checks;

namespace BackEnd.DTOs.Requests.PaymentOrder;

public class CreatePaymentOrderDto
{
    public int PurchaseOrderForSupplierId { get; set; }
    public decimal Amount { get; set; }
    public int BankAccountId { get; set; }
    public string PaymentMethod { get; set; } = "Bank";
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public CreateCheckRequestDto? CheckDetails { get; set; }
    public List<PaymentCreditNoteDto>? CreditNotes { get; set; }
}
