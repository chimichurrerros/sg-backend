using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseReturn
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public int? BillId { get; set; }

    public int BranchId { get; set; }

    public int ReasonId { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public string? Observation { get; set; }

    public decimal Total { get; set; }

    public decimal TaxTotal { get; set; }

    public PurchaseReturnStateEnum State { get; set; } = PurchaseReturnStateEnum.Created;

    public virtual Bill? Bill { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual PurchaseReturnReason Reason { get; set; } = null!;

    public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}