using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int TransactionTypeId { get; set; }

    public int? TransferId { get; set; }

    public int UserId { get; set; }

    public int StateId { get; set; }

    public DateTime Date { get; set; }

    public bool AddStock { get; set; }

    public string? Observation { get; set; }

    public virtual State State { get; set; } = null!;

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual TransactionType TransactionType { get; set; } = null!;

    public virtual Transfer? Transfer { get; set; }

    public virtual User User { get; set; } = null!;
}
