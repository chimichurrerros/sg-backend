using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Entry;

public class UpdateEntryRequestDto
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public ModuleEnum Module { get; set; }
    public int AccountantProcessId { get; set; }
    
    public List<UpdateEntryDetailDto> EntryDetails { get; set; } = new();
}
