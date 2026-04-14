using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Schedule
{
    public int Id { get; set; }

    public int ScheduleTypeId { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public decimal NumberOfHours { get; set; }

    public virtual ICollection<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; } = new List<PositionByScheduleByEmployee>();

    public virtual ScheduleType ScheduleType { get; set; } = null!;
}
