using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public PurchaseRequestStateEnum PurchaseRequestState { get; set; } = PurchaseRequestStateEnum.Pending;

    public string? Observation { get; set; }

    public virtual ICollection<PurchaseRequestDetail> PurchaseRequestDetails { get; set; } = new List<PurchaseRequestDetail>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<SupplierQuote> SupplierQuotes { get; set; } = new List<SupplierQuote>();

    public virtual User User { get; set; } = null!;
}
