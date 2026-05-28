using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class AccountPlan
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsAcceptor { get; set; }

    public int Level { get; set; }

    public virtual ICollection<EntryDetail> EntryDetails { get; set; } = new List<EntryDetail>();

    public virtual ICollection<EntryModelDetail> EntryModelDetails { get; set; } = new List<EntryModelDetail>();
}
