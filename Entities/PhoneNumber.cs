using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Entities;

[Index(nameof(Number), IsUnique = true)]
public class PhoneNumber
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    // Unique
    public string Number { get; set; } = null!;
    public Guid UserId { get; set; }
    // Relationships
    public virtual User User { get; set; } = null!;
}