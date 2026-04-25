using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Bill;

public partial class CreateBillRequestDto
{
    public BillTypeEnum BillType { get; set; }

    public int EntityId { get; set; }

    public int? SalesOrderId { get; set; }

    public int? PurchaseOrderId { get; set; }

    public string? Stamp { get; set; }

    public string Number { get; set; } = null!;

    public DateOnly Date { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? PaymentTerms { get; set; }

    public decimal Total { get; set; }

    public decimal TaxTotal { get; set; }

    public int StateId { get; set; }

    public bool IsCredit { get; set; }
}

public partial class UpdateBillRequestDto
{
    public BillTypeEnum BillType { get; set; }

    public int EntityId { get; set; }

    public int? SalesOrderId { get; set; }

    public int? PurchaseOrderId { get; set; }

    public string? Stamp { get; set; }

    public string Number { get; set; } = null!;

    public DateOnly Date { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? PaymentTerms { get; set; }

    public decimal Total { get; set; }

    public decimal TaxTotal { get; set; }

    public int StateId { get; set; }

    public bool IsCredit { get; set; }
}