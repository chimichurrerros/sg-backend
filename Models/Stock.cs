using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Stock
{
    public int Id { get; set; }

    public int WarehouseId { get; set; }

    public int LoteId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Lote Lote { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
