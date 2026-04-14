using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PaymentOrder
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public int StateId { get; set; }

    public virtual ICollection<PaymentOrderBill> PaymentOrderBills { get; set; } = new List<PaymentOrderBill>();

    public virtual ICollection<PaymentOrderMovement> PaymentOrderMovements { get; set; } = new List<PaymentOrderMovement>();

    public virtual State State { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
