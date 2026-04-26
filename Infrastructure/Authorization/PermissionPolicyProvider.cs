using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BackEnd.Infrastructure.Authorization;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy == null)
        {
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddRequirements(new PermissionRequirement(policyName));
            policy = policyBuilder.Build();
        }

        return policy;
    }
}
