using System.ComponentModel.DataAnnotations;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.DTOs.Requests.Permission;

public class PermissionRequestDto
{
    [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "El identificador del rol es obligatorio.")]
    public int RoleId { get; set; }
}

public class PermissionQueryDto : PaginationRequestDto
{
    public string? Name { get; set; }
    public int? RoleId { get; set; }
}
