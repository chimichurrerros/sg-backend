using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Warehouse
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual ICollection<Transfer> TransferDestinationWarehouses { get; set; } = new List<Transfer>();

    public virtual ICollection<Transfer> TransferSourceWarehouses { get; set; } = new List<Transfer>();
}
