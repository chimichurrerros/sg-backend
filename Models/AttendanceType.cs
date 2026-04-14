using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class AttendanceType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool AffectsPayroll { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
