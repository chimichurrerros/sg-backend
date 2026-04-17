using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Transfer
{
    public int Id { get; set; }

    public int SourceWarehouseId { get; set; }

    public int DestinationWarehouseId { get; set; }

    public int UserId { get; set; }

    public int StateId { get; set; }

    public DateTime? ShipmentDate { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public string? Observation { get; set; }

    public virtual Warehouse DestinationWarehouse { get; set; } = null!;

    public virtual Warehouse SourceWarehouse { get; set; } = null!;

    public virtual State State { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();

    public virtual User User { get; set; } = null!;
}
