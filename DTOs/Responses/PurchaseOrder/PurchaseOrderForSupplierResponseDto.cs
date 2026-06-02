using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PurchaseOrder;

public class PurchaseOrderForSupplierDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public int? SupplierQuoteDetailId { get; set; }
}

public class PurchaseOrderForSupplierResponseDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public BackEnd.DTOs.Responses.Supplier.SupplierResponseDto? Supplier { get; set; }
    public string? SupplierName { get; set; }
    public int? SupplierQuoteId { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public PurchaseOrderForSupplierStateEnum State { get; set; }
    public List<PurchaseOrderForSupplierDetailResponseDto> Details { get; set; } = [];
}

public class PurchaseOrderForSupplierWrapperDto
{
    public PurchaseOrderForSupplierResponseDto PurchaseOrderForSupplier { get; set; } = null!;
}

public class ListPurchaseOrdersForSupplierWrapperDto
{
    public List<PurchaseOrderForSupplierResponseDto> PurchaseOrdersForSupplier { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
