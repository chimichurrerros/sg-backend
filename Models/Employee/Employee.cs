using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Employee
{
    public enum GenderEnum
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum MaritalStatusEnum
    {
        Unknown = 0,
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4,
        Separated = 5,
        Cohabiting = 6
    }

    public int Id { get; set; }

    public string FileNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public GenderEnum Gender { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public int AreaId { get; set; }

    public int? InmediatlyBossId { get; set; }
    
    public int? BranchId { get; set; }

    public DateOnly HireDate { get; set; }

    public MaritalStatusEnum MaritalStatus { get; set; }

    public virtual Department Area { get; set; } = null!;

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<EmployeeRelation> EmployeeRelations { get; set; } = new List<EmployeeRelation>();

    public virtual Employee? InmediatlyBoss { get; set; }

    public virtual ICollection<Employee> InverseInmediatlyBoss { get; set; } = new List<Employee>();

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();

    public virtual ICollection<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; } = new List<PositionByScheduleByEmployee>();
}
