using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Branch;

public class BranchResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
}

public class BranchWrapperDto
{
    public BranchResponseDto Branch { get; set; } = null!;
}

public class ListBranchesWrapperDto
{
    public List<BranchResponseDto> Branches { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
