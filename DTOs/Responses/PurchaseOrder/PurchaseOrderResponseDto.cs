using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PurchaseOrder;

public class PurchaseOrderDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public int? SupplierQuoteDetailId { get; set; }
    public int? SupplierQuoteId { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
}

public class PurchaseOrderResponseDto
{
    public int Id { get; set; }
    public int PurchaseRequestId { get; set; }
    public int SupplierId { get; set; }
    public BackEnd.DTOs.Responses.Supplier.SupplierResponseDto? Supplier { get; set; }
    public string? SupplierName { get; set; }
    public int? SupplierQuoteId { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public PurchaseOrderStateEnum State { get; set; }
    public List<PurchaseOrderDetailResponseDto> Details { get; set; } = [];
}

public class PurchaseOrderWrapperDto
{
    public PurchaseOrderResponseDto PurchaseOrder { get; set; } = null!;
}

public class PurchaseOrderDraftWrapperDto
{
    public PurchaseOrderResponseDto PurchaseOrder { get; set; } = null!;
}

public class ListPurchaseOrdersWrapperDto
{
    public List<PurchaseOrderResponseDto> PurchaseOrders { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
