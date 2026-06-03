using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseOrderForSupplier
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public int SupplierId { get; set; }

    public int? SupplierQuoteId { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public PurchaseOrderForSupplierStateEnum State { get; set; } = PurchaseOrderForSupplierStateEnum.Pending;

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual SupplierQuote? SupplierQuote { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
