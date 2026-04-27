using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Bill;

public class BillResponseDto
{
    public int Id { get; set; }
    public BillTypeEnum BillType { get; set; }
    public BillStateEnum BillState { get; set; }
    public int CustomerId { get; set; }
    public int? SalesOrderId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? Stamp { get; set; }
    public string Number { get; set; } = null!;
    public DateOnly Date { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal Total { get; set; }
    public decimal TaxTotal { get; set; }
    public bool IsCredit { get; set; }
}

public class BillWrapperDto
{
    public BillResponseDto Bill { get; set; } = null!;
}

public class ListBillsWrapperDto
{
    public List<BillResponseDto> Bills { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}