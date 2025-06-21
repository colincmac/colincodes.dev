using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.Core;
using System.Text;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public sealed class ProtectedResourceJwtBearerEvents
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


    /// <summary>
    /// Add's the protected resource metadata URI to the WWW-Authenticate header in the challenge response.
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <remarks>
    /// While this can be invoked in isolation to add the resource_metadata, it is typically used in conjunction with the <see cref="JwtBearerEvents.Challenge"/> event.
    /// </remarks>
    public Task Challenge(JwtBearerChallengeContext context)
    {
        var options = _optionsMonitor.GetKeyedOrCurrent(context.Scheme.Name);

        if (options == null)
        {
            _logger.LogDebug("No ProtectedResourceOptions found for scheme: {Scheme}. Skipping challenge modification.", context.Scheme.Name);
            return Task.CompletedTask;
        }

        // TODO: handle null, validate during runtime, or throw an exception if the options are not valid.
        // This URI needs to match "<resource-host>/<default-resource-discovery-endpoint>/<optional-hosted-resource>".
        var resourceMetadataUri = options.ProtectedResourceMetadataAddress switch
        {
            { IsAbsoluteUri: true } => options.ProtectedResourceMetadataAddress, // If the path is absolute, use it directly
            _ => new Uri(GetBaseRequestUri(context.Request, context.Options.RequireHttpsMetadata), options.ProtectedResourceMetadataAddress)
        };


        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        // WWW-Authenticate: Bearer resource_metadata="https://example.com/.well-known/oauth-resource-metadata/<optional-hosted-resource>"
        var stringBuilder = new StringBuilder();
        // Add the scheme's challenge to the WWW-Authenticate header if not already present
        if (!context.Response.Headers.WWWAuthenticate.Contains(context.Options.Challenge))
        {
            stringBuilder.Append(context.Options.Challenge);
        }

        if (context.Options.Challenge.IndexOf(' ') > 0)
        {
            // Only add a comma after the first param, if any
            stringBuilder.Append(',');
        }

        stringBuilder.Append(" resource_metadata=\"");
        stringBuilder.Append(resourceMetadataUri);
        stringBuilder.Append('\"');

        _logger.LogDebug("Adding Protected Metadata to Challenge header {Header} for scheme: {Scheme}", stringBuilder.ToString(), context.Scheme.Name);

        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, stringBuilder.ToString());

        return Task.CompletedTask;
    }
    private static Uri GetBaseRequestUri(HttpRequest request, bool requireHttps) => new($"{(requireHttps ? Uri.UriSchemeHttps : request.Scheme)}://{request.Host}{request.PathBase}");

}
