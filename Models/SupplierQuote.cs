using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SupplierQuote
{
    public enum SupplierQuoteStateEnum
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int PurchaseRequestId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public SupplierQuoteStateEnum State { get; set; } = SupplierQuoteStateEnum.Pending;

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplierQuoteDetail> SupplierQuoteDetails { get; set; } = new List<SupplierQuoteDetail>();
}
