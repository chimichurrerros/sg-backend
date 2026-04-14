using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class EntryModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<EntryModelDetail> EntryModelDetails { get; set; } = new List<EntryModelDetail>();
}
