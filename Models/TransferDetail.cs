using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class TransferDetail
{
    public int Id { get; set; }

    public int TransferId { get; set; }

    public int LoteId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public virtual Lote Lote { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Transfer Transfer { get; set; } = null!;
}
