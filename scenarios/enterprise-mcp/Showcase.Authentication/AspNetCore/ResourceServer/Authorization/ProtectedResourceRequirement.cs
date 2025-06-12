using Microsoft.AspNetCore.Authorization;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;
public class ProtectedResourceScopeRequirement : IAuthorizationRequirement
{
    public string? Resource { get; }
    public IEnumerable<string> RequiredScopes { get; }

    public ProtectedResourceScopeRequirement(IEnumerable<string> requiredScopes, string? resource = null)
    {
        Resource = resource;
        RequiredScopes = requiredScopes;
    }
}
