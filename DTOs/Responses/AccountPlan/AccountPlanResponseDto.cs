using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.AccountPlan;

public class AccountPlanResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public int Order { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool IsAcceptor { get; set; }
    public int AccountantProcessId { get; set; }
}

public class AccountPlanWrapperDto
{
    public AccountPlanResponseDto AccountPlan { get; set; } = null!;
}

public class ListAccountPlansWrapperDto
{
    public List<AccountPlanResponseDto> AccountPlans { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
