using Microsoft.AspNetCore.Authorization;

namespace BackEnd.Infrastructure.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // We check if the claim "Permission" exists with the required value
        var hasPermission = context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}