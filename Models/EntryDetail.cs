using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class EntryDetail
{
    public int Id { get; set; }

    public int EntryId { get; set; }

    public int AccountPlanId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public virtual AccountPlan AccountPlan { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;
}
