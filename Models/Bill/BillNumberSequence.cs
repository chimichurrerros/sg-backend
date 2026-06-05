using BackEnd.Models;

namespace BackEnd.Models;

public partial class BillNumberSequence
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int LastNumber { get; set; }

    public virtual Branch Branch { get; set; } = null!;
}
