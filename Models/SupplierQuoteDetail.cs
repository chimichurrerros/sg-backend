using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SupplierQuoteDetail
{
    public int Id { get; set; }

    public int SupplierQuoteId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityAvailable { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SupplierQuote SupplierQuote { get; set; } = null!;
}
