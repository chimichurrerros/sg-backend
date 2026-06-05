using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseReceipt
{
    public int Id { get; set; }

    public int PurchaseOrderForSupplierId { get; set; }

    public int? BillId { get; set; }

    public int BranchId { get; set; }

    public int SupplierId { get; set; }

    public string Number { get; set; } = null!;

    public string Stamp { get; set; } = null!;

    public DateTime Date { get; set; }

    public string? Observation { get; set; }

    public decimal Total { get; set; }

    public decimal TaxTotal { get; set; }

    public virtual Bill? Bill { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual PurchaseOrderForSupplier PurchaseOrderForSupplier { get; set; } = null!;

    public virtual ICollection<PurchaseReceiptDetail> PurchaseReceiptDetails { get; set; } = new List<PurchaseReceiptDetail>();
}
