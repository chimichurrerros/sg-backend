using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.Role;

public class RoleRequestDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    public string Name { get; set; } = null!;
}

public class SyncRolePermissionsRequestDto
{
    [Required(ErrorMessage = "La lista de permisos es obligatoria.")]
    public List<string> Permissions { get; set; } = [];
}
