using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class EntryModelDetail
{
    public int Id { get; set; }

    public int EntryModelId { get; set; }

    public int AccountPlanId { get; set; }

    public bool IsDebit { get; set; }

    public virtual AccountPlan AccountPlan { get; set; } = null!;

    public virtual EntryModel EntryModel { get; set; } = null!;
}
