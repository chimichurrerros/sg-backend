using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SalesReturn
{
    public int Id { get; set; }

    public int CreditNoteId { get; set; }

    public int BillId { get; set; }

    public int SalesOrderId { get; set; }

    public string SalesOrderNumber { get; set; } = null!;

    public int BranchId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerRuc { get; set; } = null!;

    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public string Reason { get; set; } = null!;

    public virtual CreditNote CreditNote { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;
}
