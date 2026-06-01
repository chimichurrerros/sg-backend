using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.CreditNote;

public class CreditNoteQueryDto : PaginationRequestDto
{
    public string? CustomerName { get; set; }
    public string? CustomerRuc { get; set; }
    public string? BillNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
