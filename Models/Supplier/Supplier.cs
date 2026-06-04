using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public string Ruc { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public string BusinessName { get; set; } = null!;

    public string? FantasyName { get; set; }

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual ICollection<PurchaseOrderForSupplier> PurchaseOrdersForSupplier { get; set; } = new List<PurchaseOrderForSupplier>();

    public virtual ICollection<SupplierCategory> SupplierCategories { get; set; } = new List<SupplierCategory>();

    public virtual ICollection<SupplierQuote> SupplierQuotes { get; set; } = new List<SupplierQuote>();

    public virtual ICollection<RequestForQuotation> RequestForQuotations { get; set; } = new List<RequestForQuotation>();
}
