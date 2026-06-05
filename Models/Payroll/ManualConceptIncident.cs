using System;

namespace BackEnd.Models;

public partial class ManualConceptIncident
{
    public enum ManualConceptStatus
    {
        Pending = 1,
        Assigned = 2
    }

    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int PayrollUpdateId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly OccurrenceDate { get; set; }

    public ManualConceptStatus Status { get; set; }

    public int? PayrollProcessId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual PayrollProcess? PayrollProcess { get; set; }

    public virtual PayrollUpdate PayrollUpdate { get; set; } = null!;
}