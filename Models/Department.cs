using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Department
{
    public int Id { get; set; }

    public int? BossId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Employee? Boss { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
