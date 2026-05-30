using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Entry;

public class EntryDetailResponseDto
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int AccountPlanId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class EntryResponseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public ModuleEnum Module { get; set; }
    public int AccountantProcessId { get; set; }
    
    public List<EntryDetailResponseDto> EntryDetails { get; set; } = [];
}

public class EntryWrapperDto
{
    public EntryResponseDto Entry { get; set; } = null!;
}

public class ListEntriesWrapperDto
{
    public List<EntryResponseDto> Entries { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
