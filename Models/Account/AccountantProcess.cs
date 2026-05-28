using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class AccountantProcess
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int StateId { get; set; }

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public virtual State State { get; set; } = null!;
}
