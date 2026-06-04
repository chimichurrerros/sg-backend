using System;

namespace BackEnd.Models;

public partial class EmployeeRelation
{
    public enum RelationTypeEnum
    {
        Child = 1,
        Spouse = 2
    }

    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public RelationTypeEnum RelationType { get; set; }

    public string Name { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string DocumentNumber { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
