using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class CustomerQuote
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public int BranchId { get; set; }

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public decimal ImportValue { get; set; }

    public string? Number { get; set; }

    public PaymentMethodEnum? PaymentMethod { get; set; }

    public SaleConditionEnum? SaleCondition { get; set; }

    public BillTypeEnum? BillType { get; set; }

    public int? AccountId { get; set; }

    public int? MovementType { get; set; }

    public int? CashierNumber { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Open;

    public virtual Customer Customer { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<CustomerQuoteDetail> CustomerQuoteDetails { get; set; } = new List<CustomerQuoteDetail>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public enum QuoteStatus
    {
        Open = 0,
        Expired = 1,
        Closed = 2
    }
}
