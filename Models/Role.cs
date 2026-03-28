using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BackEnd.Models;

public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    //  0 -> Admin
    //  1 -> User
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    // Relationships 
    public virtual List<User>? Users { get; set; }
    public virtual List<Permission>? Permissions { get; set; }
}