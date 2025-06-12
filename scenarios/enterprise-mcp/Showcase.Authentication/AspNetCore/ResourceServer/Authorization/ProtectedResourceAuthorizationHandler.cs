using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authorization;
public class ProtectedResourceAuthorizationHandler : AuthorizationHandler<ProtectedResourceScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProtectedResourceScopeRequirement requirement)
    {
        // The resource is either the HttpContext or the Endpoint directly when used with the
        // authorization middleware
        var endpoint = context.Resource switch
        {
            HttpContext httpContext => httpContext.GetEndpoint(),
            Endpoint ep => ep,
            _ => null,
        };

        var data = endpoint?.Metadata.GetMetadata<IAuthRequiredScopeMetadata>();

        return Task.CompletedTask;
    }
}
