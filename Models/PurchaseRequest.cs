using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public int StateId { get; set; }

    public string? Observation { get; set; }

    public virtual ICollection<PurchaseRequestDetail> PurchaseRequestDetails { get; set; } = new List<PurchaseRequestDetail>();

    public virtual State State { get; set; } = null!;

    public virtual ICollection<SupplierQuote> SupplierQuotes { get; set; } = new List<SupplierQuote>();

    public virtual User User { get; set; } = null!;
}
