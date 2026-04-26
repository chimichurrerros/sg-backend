using System.ComponentModel.DataAnnotations;
namespace BackEnd.Models;

public class Permission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public int RoleId { get; set; }
    // Relationships
    public virtual Role Role { get; set; } = null!;
}