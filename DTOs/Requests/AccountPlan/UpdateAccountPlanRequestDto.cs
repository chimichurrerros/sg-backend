using BackEnd.Models;

namespace BackEnd.DTOs.Requests.AccountPlan;

public class UpdateAccountPlanRequestDto
{
    public string Code { get; set; } = null!;
    public int Order { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool IsAcceptor { get; set; }
    public int AccountantProcessId { get; set; }
}
