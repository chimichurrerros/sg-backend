using System;
using System.Collections.Generic;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Permission;

public class PermissionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
}

public class PermissionWrapperDto
{
    public PermissionResponseDto Permission { get; set; } = null!;
}

public class ListPermissionsWrapperDto
{
    public List<PermissionResponseDto> Permissions { get; set; } = [];
    public Pagination? Pagination { get; set; }
}
