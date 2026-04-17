using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CustomerQuoteDetail
{
    public int Id { get; set; }

    public int CustomerQuoteId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual CustomerQuote CustomerQuote { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
