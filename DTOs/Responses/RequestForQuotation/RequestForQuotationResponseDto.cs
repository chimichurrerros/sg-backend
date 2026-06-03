using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.RequestForQuotation;

public class RequestForQuotationProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantityRequested { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class RequestForQuotationResponseDto
{
    public int Id { get; set; }
    public int PurchaseRequestId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public RequestForQuotationStateEnum State { get; set; }
    public string? Observation { get; set; }

    public DateTime PurchaseRequestDate { get; set; }
    public PurchaseRequestStateEnum PurchaseRequestState { get; set; }
    public string? PurchaseRequestObservation { get; set; }

    public List<RequestForQuotationProductDto> Products { get; set; } = new();
}

public class RequestForQuotationWrapperDto
{
    public RequestForQuotationResponseDto RequestForQuotation { get; set; } = null!;
}

public class ListRequestForQuotationsWrapperDto
{
    public List<RequestForQuotationResponseDto> RequestForQuotations { get; set; } = new();
    public Pagination? Pagination { get; set; }
}
