using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Ruc { get; set; } = null!;

    public virtual ICollection<CustomerQuote> CustomerQuotes { get; set; } = new List<CustomerQuote>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
