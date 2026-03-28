using Microsoft.AspNetCore.Authorization;

namespace BackEnd.Infrastructure.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
