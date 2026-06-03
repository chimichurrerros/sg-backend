using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SupplierQuote
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int PurchaseRequestId { get; set; }

    public int RequestForQuotationId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public SupplierQuoteStateEnum State { get; set; } = SupplierQuoteStateEnum.Pending;

    public virtual ICollection<PurchaseOrderForSupplier> PurchaseOrdersForSupplier { get; set; } = new List<PurchaseOrderForSupplier>();

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual RequestForQuotation RequestForQuotation { get; set; } = null!;

    public virtual ICollection<SupplierQuoteDetail> SupplierQuoteDetails { get; set; } = new List<SupplierQuoteDetail>();
}
