using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Entity
{
    public int Id { get; set; }

    public int EntityTypeId { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual EntityType EntityType { get; set; } = null!;

    public virtual LegalPerson? LegalPerson { get; set; }

    public virtual PhysicalPerson? PhysicalPerson { get; set; }

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
