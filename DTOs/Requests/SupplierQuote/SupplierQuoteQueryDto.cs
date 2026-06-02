using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.SupplierQuote;

public class SupplierQuoteQueryDto : PaginationRequestDto
{
    public int? SupplierId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public int? RequestForQuotationId { get; set; }
    public int? State { get; set; }
    public DateOnly? Date { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
}
