using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class BillType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
