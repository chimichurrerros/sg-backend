using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.BillDetail;

public class BillDetailResponseDto
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
}

public class BillDetailWrapperDto
{
    public BillDetailResponseDto BillDetail { get; set; } = null!;
}

public class ListBillDetailsWrapperDto
{
    public List<BillDetailResponseDto> BillDetails { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}