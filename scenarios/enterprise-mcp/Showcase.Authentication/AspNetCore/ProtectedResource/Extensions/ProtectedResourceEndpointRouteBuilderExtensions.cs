using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Showcase.Authentication.AspNetCore.Models;
using Showcase.Authentication.AspNetCore.ProtectedResource.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Extensions;
public static class ProtectedResourceEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the resource metadata endpoint for OAuth authorization based on RFC 9728.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern. Must match the format `/.well-known/oauth-protected-resource` for a single tenant server or `/.well-known/oauth-protected-resource/hostedResourceName` for a server with multiple tenant auth configurations.</param>
    /// <param name="configure">An action to configure the resource metadata.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapProtectedResourceMetadata(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern = ProtectedResourceConstants.DefaultOAuthProtectedResourceRoute,
        Action<ProtectedResourceMetadata>? configure = null
        )
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<ProtectedResourceOptions>>();
        
        return endpoints.MapGet(pattern, async (HttpContext context, string? resource = null) =>
        {
            var lowerCaseResource = resource?.ToLowerInvariant() ?? string.Empty;
            var metadataService = string.IsNullOrEmpty(lowerCaseResource) ? context.RequestServices.GetRequiredService<ProtectedResourceMetadataService>() : context.RequestServices.GetRequiredKeyedService<ProtectedResourceMetadataService>(lowerCaseResource);
            var metadata = await metadataService.GetProtectedResourceMetadataAsync(context);

            if (metadata == null)
            {
                return Results.NotFound($"Protected resource metadata not found. Requested path: '{context.Request.Path}'");
            }
            configure?.Invoke(metadata);
            return Results.Ok(metadata);
        })
        .AllowAnonymous()
        .WithDisplayName($"Protected Resource Metadata");
    }
}
