using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.SalesReturn;

public class SalesReturnResponseDto
{
    public int Id { get; set; }
    public int CreditNoteId { get; set; }
    public int BillId { get; set; }
    public int SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRuc { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<CreditNoteDetailResponseDto> Details { get; set; } = new();
}

public class SalesReturnWrapperDto
{
    public SalesReturnResponseDto SalesReturn { get; set; } = null!;
}

public class ListSalesReturnsWrapperDto
{
    public List<SalesReturnResponseDto> SalesReturns { get; set; } = new();
    public Pagination? Pagination { get; set; }
}
