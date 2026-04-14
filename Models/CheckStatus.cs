using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CheckStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<BankMovement> BankMovements { get; set; } = new List<BankMovement>();
}
