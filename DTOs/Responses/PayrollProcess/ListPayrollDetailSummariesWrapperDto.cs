using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PayrollProcess;

public class ListPayrollDetailSummariesWrapperDto
{
    public List<PayrollDetailSummaryResponseDto> Summaries { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
