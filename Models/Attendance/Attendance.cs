using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int AttendanceTypeId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public int? MinutesLate { get; set; }

    public virtual AttendanceType AttendanceType { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
