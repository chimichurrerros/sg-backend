using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollProcessDetail
{
    public int Id { get; set; }

    public int PayrollProcessId { get; set; }

    public int EmployeeId { get; set; }

    public int PayrollUpdateId { get; set; }

    public decimal Amount { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual PayrollProcess PayrollProcess { get; set; } = null!;

    public virtual PayrollUpdate PayrollUpdate { get; set; } = null!;
}
