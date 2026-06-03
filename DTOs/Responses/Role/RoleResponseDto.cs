using System.Collections.Generic;

namespace BackEnd.DTOs.Responses.Role;

public class RoleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<string> Permissions { get; set; } = [];
}

public class RoleWrapperDto
{
    public RoleResponseDto Role { get; set; } = null!;
}

public class ListRolesWrapperDto
{
    public List<RoleResponseDto> Roles { get; set; } = [];
}
