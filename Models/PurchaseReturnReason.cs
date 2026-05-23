using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseReturnReason
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
}