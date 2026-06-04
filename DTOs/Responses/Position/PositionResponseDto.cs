using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Position;

public class PositionResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal DefaultBasicSalary { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}

public class PositionWrapperDto
{
    public PositionResponseDto Position { get; set; } = null!;
}

public class ListPositionsWrapperDto
{
    public List<PositionResponseDto> Positions { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}