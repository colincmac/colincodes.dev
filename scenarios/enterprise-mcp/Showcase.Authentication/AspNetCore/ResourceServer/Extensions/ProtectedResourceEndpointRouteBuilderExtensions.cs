using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Extensions;
public static class ProtectedResourceEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the resource metadata endpoint for OAuth authorization based on RFC 9728.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern. Must match the format `/.well-known/oauth-protected-resource` for a single tenant server or `/.well-known/oauth-protected-resource/hostedResourceName` for a server with multiple tenant auth configurations.</param>
    /// <param name="configure">An action to configure the resource metadata.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointRouteBuilder MapProtectedResourcesDiscovery(
        this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<ProtectedResourceOptions>>();
        return endpoints;
    }
}
