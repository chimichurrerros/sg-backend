using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.State;

public class StateResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class StateWrapperDto
{
    public StateResponseDto State { get; set; } = null!;
}

public class ListStatesWrapperDto
{
    public List<StateResponseDto> States { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
