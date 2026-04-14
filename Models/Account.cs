using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Account
{
    public int Id { get; set; }

    public int AccountTypeId { get; set; }

    public int? BankId { get; set; }

    public string Name { get; set; } = null!;

    public int CurrencyId { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    public virtual AccountType AccountType { get; set; } = null!;

    public virtual Bank? Bank { get; set; }

    public virtual ICollection<BankMovement> BankMovements { get; set; } = new List<BankMovement>();

    public virtual Currency Currency { get; set; } = null!;
}
