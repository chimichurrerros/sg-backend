using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PaymentOrder
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public PaymentOrderStateEnum State { get; set; }

    public virtual ICollection<PaymentOrderBill> PaymentOrderBills { get; set; } = new List<PaymentOrderBill>();

    public virtual ICollection<PaymentOrderMovement> PaymentOrderMovements { get; set; } = new List<PaymentOrderMovement>();

    public virtual ICollection<PaymentOrderCreditNote> PaymentOrderCreditNotes { get; set; } = new List<PaymentOrderCreditNote>();

    public virtual Supplier Supplier { get; set; } = null!;
}
