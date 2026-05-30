namespace BackEnd.Models;

public partial class Entry
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public ModuleEnum Module { get; set; }

    public int AccountantProcessId { get; set; }

    public virtual AccountantProcess AccountantProcess { get; set; } = null!;

    public virtual ICollection<EntryDetail> EntryDetails { get; set; } = new List<EntryDetail>();

}