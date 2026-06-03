namespace BackEnd.Models;

public partial class PurchaseReceiptDetail
{
    public int Id { get; set; }

    public int PurchaseReceiptId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseReceipt PurchaseReceipt { get; set; } = null!;
}
