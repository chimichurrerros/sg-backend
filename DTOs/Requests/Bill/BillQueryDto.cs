using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.Bill;

public class BillQueryDto : PaginationRequestDto
{
    public string? CustomerName { get; set; }
    public int? CustomerId { get; set; }
    public string? Number { get; set; }
    public DateOnly? Date { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? CustomerRuc { get; set; }
}
