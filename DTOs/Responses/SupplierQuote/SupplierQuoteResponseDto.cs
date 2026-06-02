using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.SupplierQuote;

public class SupplierQuoteDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal ProductTaxRate { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal Price { get; set; }
}

public class SupplierQuoteResponseDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int PurchaseRequestId { get; set; }
    public int RequestForQuotationId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public SupplierQuoteStateEnum State { get; set; }
    public List<SupplierQuoteDetailResponseDto> Details { get; set; } = [];
    
    /// <summary>
    /// If the supplier quote has an associated purchase order, contains its id; otherwise null.
    /// Useful to link from the quote to the related purchase order.
    /// </summary>
    public int? AssociatedPurchaseOrderId { get; set; }
}

public class SupplierQuoteWrapperDto
{
    public SupplierQuoteResponseDto SupplierQuote { get; set; } = null!;
}

public class ListSupplierQuotesWrapperDto
{
    public List<SupplierQuoteResponseDto> SupplierQuotes { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
