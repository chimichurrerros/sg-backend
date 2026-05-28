using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class ScheduleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
