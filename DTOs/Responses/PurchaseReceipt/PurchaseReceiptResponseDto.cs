namespace BackEnd.DTOs.Responses.PurchaseReceipt;

public class PurchaseReceiptDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class PurchaseReceiptResponseDto
{
    public int Id { get; set; }
    public int PurchaseOrderForSupplierId { get; set; }
    public int? BillId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Stamp { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Observation { get; set; }
    public decimal Total { get; set; }
    public decimal TaxTotal { get; set; }
    public List<PurchaseReceiptDetailResponseDto> Details { get; set; } = [];
}

public class PurchaseReceiptWrapperDto
{
    public PurchaseReceiptResponseDto PurchaseReceipt { get; set; } = null!;
}

public class ListPurchaseReceiptsWrapperDto
{
    public List<PurchaseReceiptResponseDto> PurchaseReceipts { get; set; } = [];
    public Utils.Pagination? Pagination { get; set; }
}
