using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Employee
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public string FileNumber { get; set; } = null!;

    public int AreaId { get; set; }

    public int? InmediatlyBossId { get; set; }

    public DateOnly HireDate { get; set; }

    public virtual Department Area { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<EmployeeKid> EmployeeKids { get; set; } = new List<EmployeeKid>();

    public virtual PhysicalPerson Entity { get; set; } = null!;

    public virtual Employee? InmediatlyBoss { get; set; }

    public virtual ICollection<Employee> InverseInmediatlyBoss { get; set; } = new List<Employee>();

    public virtual ICollection<PayrollProcessDetail> PayrollProcessDetails { get; set; } = new List<PayrollProcessDetail>();

    public virtual ICollection<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; } = new List<PositionByScheduleByEmployee>();
}
