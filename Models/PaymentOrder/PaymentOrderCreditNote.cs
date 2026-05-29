using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PaymentOrderCreditNote
{
    public int Id { get; set; }

    public int PaymentOrderId { get; set; }

    public int CreditNoteId { get; set; }

    public decimal Amount { get; set; }

    public virtual CreditNote CreditNote { get; set; } = null!;

    public virtual PaymentOrder PaymentOrder { get; set; } = null!;
}
