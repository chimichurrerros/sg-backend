using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Bank
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string AccountNumber {get; set;} = null!;

    public BankMovementTypeEnum AccountType {get; set;}

    public string? Ruc {get; set;}

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
