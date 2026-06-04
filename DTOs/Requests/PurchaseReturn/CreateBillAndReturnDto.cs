namespace BackEnd.DTOs.Requests.PurchaseReturn;

public class CreateBillDto
{
    public int PurchaseOrderForSupplierId { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public decimal TaxTotal { get; set; }
    public string? Notes { get; set; }
}

public class CreateBillAndReturnDto
{
    public CreateBillDto Bill { get; set; } = null!;
    public CreatePurchaseReturnDto Return { get; set; } = null!;
}
