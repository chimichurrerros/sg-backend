using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class ProductBrand
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
