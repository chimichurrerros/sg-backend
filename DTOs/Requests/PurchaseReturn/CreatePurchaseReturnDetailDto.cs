namespace BackEnd.DTOs.Requests.PurchaseReturn;

public class CreatePurchaseReturnDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}