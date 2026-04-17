using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CreditNoteDetail
{
    public int Id { get; set; }

    public int CreditNoteId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual CreditNote CreditNote { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
