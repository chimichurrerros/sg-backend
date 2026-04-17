using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseRequestDetail
{
    public int Id { get; set; }

    public int PurchaseRequestId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityRequested { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;
}
