using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Stock
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    
    public int BranchId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;
    
    public virtual Branch Branch { get; set; } = null!;
}
