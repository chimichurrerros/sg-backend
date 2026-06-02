using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PurchaseReturn;

public class PurchaseReturnReasonResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class PurchaseReturnDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class PurchaseReturnResponseDto
{
    public int Id { get; set; }
    public int PurchaseOrderForSupplierId { get; set; }
    public int? BillId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Observation { get; set; }
    public decimal Total { get; set; }
    public decimal TaxTotal { get; set; }
    public PurchaseReturnStateEnum State { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<PurchaseReturnDetailResponseDto> Details { get; set; } = [];
}

public class PurchaseReturnWrapperDto
{
    public PurchaseReturnResponseDto PurchaseReturn { get; set; } = null!;
}

public class ListPurchaseReturnsWrapperDto
{
    public List<PurchaseReturnResponseDto> PurchaseReturns { get; set; } = [];
    public Pagination? Pagination { get; set; }
}

public class PurchaseReturnReasonWrapperDto
{
    public PurchaseReturnReasonResponseDto Reason { get; set; } = null!;
}

public class ListPurchaseReturnReasonsWrapperDto
{
    public List<PurchaseReturnReasonResponseDto> Reasons { get; set; } = [];
}