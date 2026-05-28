using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollProcess
{
    public int Id { get; set; }

    public int PayrollStatusId { get; set; }

    public int ProcessTypeId { get; set; }

    public string Name { get; set; } = null!;

    public int Year { get; set; }

    public int Month { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? PayDate { get; set; }

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();

    public virtual PayrollStatus PayrollStatus { get; set; } = null!;

    public virtual ProcessType ProcessType { get; set; } = null!;
}
