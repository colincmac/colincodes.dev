using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
