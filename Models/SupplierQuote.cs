using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SupplierQuote
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int PurchaseRequestId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public int StateId { get; set; }

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual State State { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplierQuoteDetail> SupplierQuoteDetails { get; set; } = new List<SupplierQuoteDetail>();
}
