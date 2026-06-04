using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Entry;

public class UpdateEntryDetailDto
{
    public int? Id { get; set; }
    public int AccountPlanId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
