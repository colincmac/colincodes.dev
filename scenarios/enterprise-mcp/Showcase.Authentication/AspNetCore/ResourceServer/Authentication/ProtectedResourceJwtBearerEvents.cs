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

    private static Uri GetBaseRequestUri(HttpRequest request, bool requireHttps) => new ($"{(requireHttps ? Uri.UriSchemeHttps : request.Scheme)}://{request.Host}{request.PathBase}");

    public Task Challenge(JwtBearerChallengeContext context)
    {
        var options = _optionsMonitor.Get(context.Scheme.Name);
        
        if (options == null)
        {
            _logger.LogDebug("No ProtectedResourceOptions found for scheme: {Scheme}. Skipping challenge modification.", context.Scheme.Name);
            return Task.CompletedTask;
        }

        // This URI needs to match "<resource-host>/<default-resource-discovery-endpoint>/<optional-hosted-resource>". It's up to the client to verify whether these match.
        Uri resourceMetadataUri = options.ProtectedMetadataPath switch
        {
            { IsAbsoluteUri: true } => options.ProtectedMetadataPath, // If the path is absolute, use it directly
            _ => new Uri(GetBaseRequestUri(context.Request, context.Options.RequireHttpsMetadata), options.ProtectedMetadataPath)
        };

        _logger.LogDebug("Adding Protected Metadata to Challenge header for scheme: {Scheme}", context.Scheme.Name);
        
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;


        // WWW-Authenticate: Bearer resource_metadata="https://example.com/.well-known/oauth-resource-metadata/<optional-hosted-resource>"
        var stringBuilder = new StringBuilder();
        // Add the scheme's challenge to the WWW-Authenticate header if not already present
        if (context.Response.Headers.WWWAuthenticate.Any(header => header?.Contains(context.Options.Challenge) ?? false))
        {
            stringBuilder.Append(context.Options.Challenge);
            if (context.Options.Challenge.IndexOf(' ') > 0)
            {
                // Only add a comma after the first param, if any
                stringBuilder.Append(',');
            }
        }

        stringBuilder.Append(" resource_metadata=\"");
        stringBuilder.Append(resourceMetadataUri);
        stringBuilder.Append('\"');

        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, stringBuilder.ToString());

        return Task.CompletedTask;
    }

}
