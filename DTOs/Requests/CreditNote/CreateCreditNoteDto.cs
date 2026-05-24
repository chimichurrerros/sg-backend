namespace BackEnd.DTOs.Requests.CreditNote;

public class CreateCreditNoteDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreateCreditNoteDto
{
    public int BillId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<CreateCreditNoteDetailDto> Details { get; set; } = new List<CreateCreditNoteDetailDto>();
}
