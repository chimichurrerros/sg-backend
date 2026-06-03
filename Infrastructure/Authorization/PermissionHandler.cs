using System.Security.Claims;
using BackEnd.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackEnd.Infrastructure.Authorization;

public class PermissionHandler(IServiceProvider serviceProvider) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return;
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Check if the user is active and has a role that possesses this permission
        var hasPermission = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .AnyAsync(u => u.Role.Permissions.Any(p => p.Name == requirement.Permission));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}