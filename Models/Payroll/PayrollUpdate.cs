using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PayrollUpdate
{
    public enum PayrollTypeEnum
    {
        Earnings = 1,
        Deductions = 2
    }

    public enum FormulaTypeEnum
    {
        Fixed = 1,
        Calculated = 2
    }

    public int Id { get; set; }

    public PayrollTypeEnum PayrollTypeId { get; set; }

    public FormulaTypeEnum FormulaTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Formula { get; set; }

    public bool IpsDeductible { get; set; }

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();
}
