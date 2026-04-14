using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Module
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
