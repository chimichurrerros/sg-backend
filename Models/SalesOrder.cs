using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SalesOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public int? CustomerQuoteId { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public int StateId { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual CustomerQuote? CustomerQuote { get; set; }

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual State State { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
