using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.PurchaseRequest;

public class PurchaseRequestQueryDto : PaginationRequestDto
{
    public int? State { get; set; }
    public int? BranchId { get; set; }
}
