using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.User;

public class UpdateUserRoleRequestDto
{
    [Required(ErrorMessage = "El identificador del rol es obligatorio.")]
    public int RoleId { get; set; }
}
