using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PaymentOrder;

public class PaymentOrderBillDto
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public int PurchaseOrderId { get; set; }
    public decimal Amount { get; set; }
    public string BillNumber { get; set; } = string.Empty;
}

public class PaymentOrderMovementDto
{
    public int Id { get; set; }
    public int BankMovementId { get; set; }
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

public class PaymentOrderResponseDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int PurchaseOrderId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string StateId { get; set; } = string.Empty;
    public List<PaymentOrderBillDto> Bills { get; set; } = [];
    public List<PaymentOrderMovementDto> Movements { get; set; } = [];
}

public class PaymentOrderWrapperDto
{
    public PaymentOrderResponseDto PaymentOrder { get; set; } = null!;
}

public class ListPaymentOrdersWrapperDto
{
    public List<PaymentOrderResponseDto> PaymentOrders { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
