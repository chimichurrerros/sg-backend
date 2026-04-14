using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class EmployeeKid
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int EntityId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual PhysicalPerson Entity { get; set; } = null!;
}
