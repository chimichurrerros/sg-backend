using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.PurchaseOrder;

public class PurchaseOrderForSupplierQueryDto : PaginationRequestDto
{
    public int? PurchaseOrderId { get; set; }
    public int? SupplierId { get; set; }
    public int? State { get; set; }
    public DateOnly? Date { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
}
