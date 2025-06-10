using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.Core;
using System.Net.Http;
using System.Text;
using System.Text.Json;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
internal class ProtectedResourceJwtBearerEvents
{
    private readonly IOptionsMonitor<ProtectedResourceOptions> _optionsMonitor;
    private readonly ILogger<ProtectedResourceJwtBearerEvents> _logger;

    public ProtectedResourceJwtBearerEvents(
        IOptionsMonitor<ProtectedResourceOptions> protectedResourceOptionsMonitor, 
        ILogger<ProtectedResourceJwtBearerEvents> logger
        )
    {
        _optionsMonitor = protectedResourceOptionsMonitor;
        _logger = logger;
    }

    private static Uri GetBaseRequestUri(HttpRequest request) => new ($"{Uri.UriSchemeHttps}://{request.Host}{request.PathBase}");

    public Task Challenge(JwtBearerChallengeContext context)
    {
        var options = _optionsMonitor.Get(context.Scheme.Name);

        if(options == null)
        {
            _logger.LogDebug("No ProtectedResourceOptions found for scheme: {Scheme}. Skipping challenge modification.", context.Scheme.Name);
            return Task.CompletedTask;
        }

        // This URI needs to match "<resource-host>/<default-resource-discovery-endpoint>/<optional-hosted-resource>". It's up to the client to verify whether these match.
        Uri resourceMetadataUri = options.ProtectedMetadataDiscoveryUri switch
        {
            { IsAbsoluteUri: true } => options.ProtectedMetadataDiscoveryUri, // If the path is absolute, use it directly
            _ => new Uri(GetBaseRequestUri(context.Request), options.ProtectedMetadataDiscoveryUri)
        };

        _logger.LogDebug("Adding Protected Metadata to Challenge header for scheme: {Scheme}", context.Scheme.Name);
        var resourceUrl = options.ProtectedMetadataDiscoveryUri.IsAbsoluteUri;
        var hostedResourcePath = options.HostedResourcePath;
        
        if(Uri.TryCreate(host, UriKind.Absolute, out var resourceHostUri) && hostedResourcePath != null)
        {
            // If the resource host is absolute and the hosted resource path is provided, combine them
            resourceHostUri = new Uri(resourceHostUri, hostedResourcePath);
        }
        else if(hostedResourcePath != null)
        {
            // If the resource host is not absolute, we assume it's a relative path
            resourceHostUri = new Uri(GetBaseRequestUri(context.Request), hostedResourcePath);
        }


        if(!resourceMetadataUri.IsAbsoluteUri)
        {
            _logger.LogWarning("The provided well-known endpoint for resource_metadata needs to be absolute. Provided URL: {Path}. This may lead to incorrect resource metadata URI.", resourceMetadataUri);
        }

        // For example:
        // WWW-Authenticate: Bearer resource_metadata="https://example.com/.well-known/oauth-resource-metadata/<optional-hosted-resource>"
        var wwwAuthenticateHeaderValue = $"{ProtectedResourceConstants.WWWAuthenticateKeys.UnsignedResourceMetadata}=\"{resourceMetadataUri.ToString()}\"";
        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, wwwAuthenticateHeaderValue);
        return Task.CompletedTask;
    }

}
