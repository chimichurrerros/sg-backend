using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class RequestForQuotation
{
    public int Id { get; set; }

    public int PurchaseRequestId { get; set; }

    public int SupplierId { get; set; }

    public DateTime Date { get; set; }

    public string? Observation { get; set; }

    public RequestForQuotationStateEnum State { get; set; } = RequestForQuotationStateEnum.Pending;

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<RequestForQuotationDetail> RequestForQuotationDetails { get; set; } = new List<RequestForQuotationDetail>();
}
