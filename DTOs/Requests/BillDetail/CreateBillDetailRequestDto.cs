namespace BackEnd.DTOs.Requests.BillDetail;

public partial class CreateBillDetailRequestDto
{
    public int BillId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }
}

public partial class UpdateBillDetailRequestDto
{
    public int BillId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxRate { get; set; }
}