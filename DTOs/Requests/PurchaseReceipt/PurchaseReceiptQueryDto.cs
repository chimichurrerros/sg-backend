using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.PurchaseReceipt;

public class PurchaseReceiptQueryDto : PaginationRequestDto
{
    public int? PurchaseOrderForSupplierId { get; set; }
    public int? BranchId { get; set; }
    public int? SupplierId { get; set; }
    public DateOnly? Date { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
