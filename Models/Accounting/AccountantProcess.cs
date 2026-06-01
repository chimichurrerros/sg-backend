namespace BackEnd.Models;

public partial class AccountantProcess
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsClosed { get; set; }

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

}