using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Gender
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PhysicalPerson> PhysicalPeople { get; set; } = new List<PhysicalPerson>();
}
