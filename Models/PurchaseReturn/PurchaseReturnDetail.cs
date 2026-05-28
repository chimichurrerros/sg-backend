using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseReturnDetail
{
    public int Id { get; set; }

    public int PurchaseReturnId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
}