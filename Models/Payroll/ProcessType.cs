using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class ProcessType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PayrollProcess> PayrollProcesses { get; set; } = new List<PayrollProcess>();
}
