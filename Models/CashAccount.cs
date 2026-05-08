namespace BackEnd.Models;

public partial class CashAccount
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }
    public virtual Branch Branch { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
}