using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class TaxCondition
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
