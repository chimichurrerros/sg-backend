using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class UnitsOfMeasurement
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
