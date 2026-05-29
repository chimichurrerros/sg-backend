using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Employee;

public class EmployeeRelationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public BackEnd.Models.EmployeeRelation.RelationTypeEnum RelationType { get; set; }
    public string Name { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class EmployeeRelationWrapperDto
{
    public EmployeeRelationResponseDto Relation { get; set; } = null!;
}

public class ListEmployeeRelationsWrapperDto
{
    public List<EmployeeRelationResponseDto> Relations { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
