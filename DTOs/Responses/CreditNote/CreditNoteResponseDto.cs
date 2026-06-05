using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.CreditNote;

public class CreditNoteDetailResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreditNoteResponseDto
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public string? Number { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public CreditNoteTypeEnum Type { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerRuc { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<CreditNoteDetailResponseDto> Details { get; set; } = new List<CreditNoteDetailResponseDto>();
}

public class CreditNoteWrapperDto
{
    public CreditNoteResponseDto CreditNote { get; set; } = null!;
}

public class ListCreditNotesWrapperDto
{
    public List<CreditNoteResponseDto> CreditNotes { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
