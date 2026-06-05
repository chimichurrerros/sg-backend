using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.RequestForQuotation;

public class RequestForQuotationQueryDto : PaginationRequestDto
{
    public int? SupplierId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public int? State { get; set; }
}
