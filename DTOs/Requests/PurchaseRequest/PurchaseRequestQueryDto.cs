using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.PurchaseRequest;

public class PurchaseRequestQueryDto : PaginationRequestDto
{
    public int? State { get; set; }
}
