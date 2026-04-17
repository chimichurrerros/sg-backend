using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class PhysicalPerson
{
    public int EntityId { get; set; }

    public int GenderId { get; set; }

    public int MaritalStatusId { get; set; }

    public string Name { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public virtual ICollection<EmployeeKid> EmployeeKids { get; set; } = new List<EmployeeKid>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Entity Entity { get; set; } = null!;

    public virtual Gender Gender { get; set; } = null!;

    public virtual MaritalStatus MaritalStatus { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
