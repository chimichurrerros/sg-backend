using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollUpdate
{
    public int Id { get; set; }

    public int PayrollTypeId { get; set; }

    public int FormulaTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Formula { get; set; }

    public bool IpsDeductible { get; set; }

    public virtual FormulaType FormulaType { get; set; } = null!;

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();

    public virtual PayrollType PayrollType { get; set; } = null!;
}
