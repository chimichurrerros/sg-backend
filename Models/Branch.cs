using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Branch
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    //DEPRECATED
    //public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
