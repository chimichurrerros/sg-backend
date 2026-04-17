using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PaymentOrderMovement
{
    public int Id { get; set; }

    public int PaymentOrderId { get; set; }

    public int BankMovementId { get; set; }

    public decimal Amount { get; set; }

    public virtual BankMovement BankMovement { get; set; } = null!;

    public virtual PaymentOrder PaymentOrder { get; set; } = null!;
}
