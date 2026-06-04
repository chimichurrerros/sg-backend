namespace BackEnd.DTOs.Requests.PurchaseReceipt;

public class CreatePurchaseReceiptDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}
