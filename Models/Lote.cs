using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Lote
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string LoteNumber { get; set; } = null!;

    public DateOnly? DueDate { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();
}
