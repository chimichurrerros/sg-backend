using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class LegalPerson
{
    public int EntityId { get; set; }

    public string BussinessName { get; set; } = null!;

    public string? FantasyName { get; set; }

    public virtual Entity Entity { get; set; } = null!;
}
