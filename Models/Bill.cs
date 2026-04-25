using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Bill
{
    public int Id { get; set; }

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

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

    public virtual Entity Entity { get; set; } = null!;

    public virtual ICollection<PaymentOrderBill> PaymentOrderBills { get; set; } = new List<PaymentOrderBill>();

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual SalesOrder? SalesOrder { get; set; }

    public virtual State State { get; set; } = null!;
}