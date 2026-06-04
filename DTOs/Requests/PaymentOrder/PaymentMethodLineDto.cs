using BackEnd.DTOs.Requests.Checks;

namespace BackEnd.DTOs.Requests.PaymentOrder;

public class PaymentMethodLineDto
{
    public string Method { get; set; } = "Transfer";
    public int? AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public CreateCheckRequestDto? CheckDetails { get; set; }
    public int? CreditNoteId { get; set; }
}
