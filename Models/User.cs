using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class User
{
    public int Id { get; set; }
    public int? EntityId { get; set; }
    public int RoleId { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<CustomerQuote> CustomerQuotes { get; set; } = new List<CustomerQuote>();

    public virtual PhysicalPerson? Entity { get; set; }

    public virtual ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
}
