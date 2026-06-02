using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.SalesReturn;

public class SalesReturnQueryDto : PaginationRequestDto
{
    public string? SalesOrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerRuc { get; set; }
    public int? BranchId { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
