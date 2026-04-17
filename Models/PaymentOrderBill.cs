using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PaymentOrderBill
{
    public int Id { get; set; }

    public int PaymentOrderId { get; set; }

    public int BillId { get; set; }

    public decimal Amount { get; set; }

    public virtual Bill Bill { get; set; } = null!;

    public virtual PaymentOrder PaymentOrder { get; set; } = null!;
}
