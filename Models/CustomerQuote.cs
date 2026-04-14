using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CustomerQuote
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public int StateId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<CustomerQuoteDetail> CustomerQuoteDetails { get; set; } = new List<CustomerQuoteDetail>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual State State { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
