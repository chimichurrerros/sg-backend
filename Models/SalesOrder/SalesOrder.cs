using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SalesOrder
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int UserId { get; set; }
    public int BranchId {get; set;}

    public int? CustomerQuoteId { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public decimal ImportValue {get; set;}

    public decimal Total { get; set; }

    public SalesOrderStateEnum SalesOrderState { get; set; }

    public PaymentMethodEnum? PaymentMethod { get; set; }

    public SaleConditionEnum? SaleCondition { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual Customer? Customer { get; set; } = null!;

    public virtual CustomerQuote? CustomerQuote { get; set; }

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual User User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}

public enum PaymentMethodEnum
{
    Cash = 1,
    Card = 2,
    Transfer = 3
}

public enum SaleConditionEnum
{
    Cash = 1,
    Credit = 2
}
