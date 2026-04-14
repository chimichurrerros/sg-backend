using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Position
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; } = new List<PositionByScheduleByEmployee>();
}
