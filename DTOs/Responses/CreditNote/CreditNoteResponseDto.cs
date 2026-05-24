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
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<CreditNoteDetailResponseDto> Details { get; set; } = new List<CreditNoteDetailResponseDto>();
}

public class CreditNoteWrapperDto
{
    public CreditNoteResponseDto CreditNote { get; set; } = null!;
}
