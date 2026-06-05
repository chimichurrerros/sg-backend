using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseOrder
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public int PurchaseRequestId { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public PurchaseOrderStateEnum State { get; set; } = PurchaseOrderStateEnum.Pending;

    public virtual Branch Branch { get; set; } = null!;

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual ICollection<PurchaseOrderForSupplier> PurchaseOrdersForSupplier { get; set; } = new List<PurchaseOrderForSupplier>();
}
