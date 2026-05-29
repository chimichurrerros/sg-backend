using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Department;

public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class DepartmentWrapperDto
{
    public DepartmentResponseDto Department { get; set; } = null!;
}

public class ListDepartmentsWrapperDto
{
    public List<DepartmentResponseDto> Departments { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}