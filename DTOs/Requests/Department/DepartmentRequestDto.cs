namespace BackEnd.DTOs.Requests.Department;

public class DepartmentRequestDto
{
    public string Name { get; set; } = null!;
    public int? BossId { get; set; }
}