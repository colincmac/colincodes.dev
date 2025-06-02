using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.AspNetCore.ProtectedResource.Services;
using System.Text.Encodings.Web;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Extensions;
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Updates the JWTBearer Options to add a protected resource metadata URL into challenge responses.
    /// </summary>
    public static AuthenticationBuilder AsProtectedResource(
        this AuthenticationBuilder builder,
        string displayName,
        string scheme = JwtBearerDefaults.AuthenticationScheme,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IProtectedResourceMetadataService, ProtectedResourceMetadataService>();
        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, (options) => 
        {
            if(configureOptions is not null) configureOptions(options);
            var originalOnChallenge = options.Events.OnChallenge;
            options.Events.OnChallenge += async (context) =>
            {

                var metadataService = context.HttpContext.RequestServices.GetRequiredService<IProtectedResourceMetadataService>();
                try
                {
                    var url = $"{context.HttpContext}";

                    /**
                     * TODO: Investigate whether we need to add additional logic for the WWW-Authenticate header based on the default JWT Bearer handler.
                     * The JWT Bearer handler invokes default handler logic before events, adding it's own `Bearer realm...` context, resulting in a WWW-Authenticate header like `Bearer realm="https", resource_metadata="https://example.com/.well-known/oauth-protected-resource". 
                     */
                    var resourceMetadataString = $"{ ProtectedResourceConstants.WWWAuthenticateKeys.ResourceMetadata }=\"{UrlEncoder.Default.Encode(url)}\"";
                    if(!context.Response.Headers.WWWAuthenticate.Contains(JwtBearerDefaults.AuthenticationScheme)) context.Response.Headers.WWW
                        ;
                    context.Response.Headers.AppendCommaSeparatedValues(HeaderNames.WWWAuthenticate,
                        $"Bearer realm=\"{context.Scheme.Name}\",  ;

                    originalOnChallenge?.Invoke(context);
                }
                catch
                {
                    // ignore metadata errors
                }
            };
        });

        return builder;
    }
}
