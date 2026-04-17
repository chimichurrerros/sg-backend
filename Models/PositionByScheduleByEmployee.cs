using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PositionByScheduleByEmployee
{
    public int Id { get; set; }

    public int PositionId { get; set; }

    public int ScheduleId { get; set; }

    public int EmployeeId { get; set; }

    public decimal BasicSalary { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Position Position { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;
}
