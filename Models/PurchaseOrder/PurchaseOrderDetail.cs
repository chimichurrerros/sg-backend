using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PurchaseOrderDetail
{
    public int Id { get; set; }

    public int PurchaseOrderForSupplierId { get; set; }

    public int? SupplierQuoteDetailId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal QuantityReturned { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SupplierQuoteDetail? SupplierQuoteDetail { get; set; }

    public virtual PurchaseOrderForSupplier PurchaseOrderForSupplier { get; set; } = null!;
}
