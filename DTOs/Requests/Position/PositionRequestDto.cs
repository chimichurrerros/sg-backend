namespace BackEnd.DTOs.Requests.Position;

public class PositionRequestDto
{
    public string Name { get; set; } = null!;
    public decimal DefaultBasicSalary { get; set; }
}