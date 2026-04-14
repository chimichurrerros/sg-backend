using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Customer
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public int TaxConditionId { get; set; }

    public decimal CreditLimit { get; set; }

    public virtual ICollection<CustomerQuote> CustomerQuotes { get; set; } = new List<CustomerQuote>();

    public virtual Entity Entity { get; set; } = null!;

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual TaxCondition TaxCondition { get; set; } = null!;
}
