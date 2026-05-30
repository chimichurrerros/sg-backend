using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class AccountPlan
{
    public int Id { get; set; } // ID
    public string Code { get; set; } = null!; // CODIGO 1, 1.1, 1.2, 1.3 
    public int Order { get; set; }
    public string Name { get; set; } = null!;   // NOMBRE
    public int? ParentId { get; set; } // ID_PADRE
    public bool IsAcceptor { get; set; } // ES IMPUTABLE
    public int AccountantProcessId { get; set; } // ID_PERIODO


    public virtual AccountPlan? Parent { get; set; } // ID_PADRE
    public virtual AccountantProcess AccountantProcess { get; set; } = null!; // ID_PERIODO
    public virtual ICollection<EntryDetail> EntryDetails { get; set; } = new List<EntryDetail>();

    public virtual ICollection<EntryModelDetail> EntryModelDetails { get; set; } = new List<EntryModelDetail>();
}
