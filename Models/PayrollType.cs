using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PayrollUpdate> PayrollUpdates { get; set; } = new List<PayrollUpdate>();
}
