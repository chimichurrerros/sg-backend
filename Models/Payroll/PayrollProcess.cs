using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollProcess
{
    public enum ProcessTypeEnum
    {
        Monthly = 1,
        Bonus = 2,
        Settlement = 3
    }

    public enum PayrollStatusEnum
    {
        Open = 1,
        Closed = 2,
        Paid = 3
    }

    public int Id { get; set; }

    public PayrollStatusEnum PayrollStatusId { get; set; }

    public ProcessTypeEnum ProcessTypeId { get; set; }

    public string Name { get; set; } = null!;

    public int Year { get; set; }

    public int Month { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? PayDate { get; set; }

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();
}
