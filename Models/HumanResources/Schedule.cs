using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public enum ScheduleTypeEnum
{
    Unknown = 0,
    Morning = 1,
    Afternoon = 2,
    Night = 3,
    FullTime = 4,
    PartTime = 5
}

public partial class Schedule
{
    public int Id { get; set; }

    public ScheduleTypeEnum ScheduleType { get; set; }

    public TimeOnly ArrivalTime { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public decimal NumberOfHours { get; set; }

    public virtual ICollection<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; } = new List<PositionByScheduleByEmployee>();
}
