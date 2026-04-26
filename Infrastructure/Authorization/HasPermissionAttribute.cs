using Microsoft.AspNetCore.Authorization;

namespace BackEnd.Infrastructure.Authorization;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission)
{
}
