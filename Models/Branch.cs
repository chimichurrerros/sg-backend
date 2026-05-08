using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Branch
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;
    
    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}