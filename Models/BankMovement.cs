using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class BankMovement
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public BankMovementTypeEnum MovementType { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceNumber { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<PaymentOrderMovement> PaymentOrderMovements { get; set; } = new List<PaymentOrderMovement>();
}
