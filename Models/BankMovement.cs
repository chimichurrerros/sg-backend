using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class BankMovement
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int MovementTypeId { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceNumber { get; set; }

    public int? CheckStatusId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual CheckStatus? CheckStatus { get; set; }

    public virtual MovementType MovementType { get; set; } = null!;

    public virtual ICollection<PaymentOrderMovement> PaymentOrderMovements { get; set; } = new List<PaymentOrderMovement>();
}
