using System;
using System.Collections.Generic;
using Microsoft.OpenApi;

namespace BackEnd.Models;

public partial class State
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountantProcess> AccountantProcesses { get; set; } = new List<AccountantProcess>();

    public virtual ICollection<CustomerQuote> CustomerQuotes { get; set; } = new List<CustomerQuote>();

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();

    public virtual ICollection<SupplierQuote> SupplierQuotes { get; set; } = new List<SupplierQuote>();
}
