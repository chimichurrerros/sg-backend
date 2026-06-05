using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class RequestForQuotationDetail
{
    public int Id { get; set; }

    public int RequestForQuotationId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityRequested { get; set; }

    public virtual RequestForQuotation RequestForQuotation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
