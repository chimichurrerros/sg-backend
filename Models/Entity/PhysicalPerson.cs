using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PhysicalPerson
{
    public int EntityId { get; set; }

    public string Name { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public virtual Entity Entity { get; set; } = null!;
}
