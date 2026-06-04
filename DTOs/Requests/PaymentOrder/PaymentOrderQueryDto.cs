using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.PaymentOrder;

public class PaymentOrderQueryDto : PaginationRequestDto
{
    public int? SupplierId { get; set; }
    public int? State { get; set; }
    public int? PurchaseOrderForSupplierId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
