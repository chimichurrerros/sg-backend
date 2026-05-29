namespace BackEnd.Models;

public partial class BranchDepartment
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int DepartmentId { get; set; }
    public int? BossId { get; set; }

    public virtual Branch Branch { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual Employee? Boss { get; set; }
}