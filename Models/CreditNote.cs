using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CreditNote
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public string Reason { get; set; } = null!;

    public virtual Bill Bill { get; set; } = null!;

    public virtual ICollection<CreditNoteDetail> CreditNoteDetails { get; set; } = new List<CreditNoteDetail>();
}
