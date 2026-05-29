using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SalesOrderDetail
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityInvoiced { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SalesOrder SalesOrder { get; set; } = null!;
}
