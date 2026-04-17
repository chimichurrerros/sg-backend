using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<SupplierCategory> SupplierCategories { get; set; } = new List<SupplierCategory>();

    public virtual ICollection<SupplierQuote> SupplierQuotes { get; set; } = new List<SupplierQuote>();
}
