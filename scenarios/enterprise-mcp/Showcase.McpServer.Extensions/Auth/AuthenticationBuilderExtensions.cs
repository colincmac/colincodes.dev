using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth;
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication and injects the protected resource metadata URL into challenge responses.
    /// </summary>
    /// <summary>
    /// Adds JWT Bearer authentication and injects the protected resource metadata URL into challenge responses.
    /// </summary>
    public static AuthenticationBuilder AddProtectedResourceJwtBearer(
        this AuthenticationBuilder builder,
        string scheme,
        string displayName,
        Action<JwtBearerOptions> configureOptions)
    {
        builder.AddJwtBearer(scheme, options =>
        {
            configureOptions(options);

            var originalOnChallenge = options.Events.OnChallenge;
            options.Events.OnChallenge = async context =>
            {
                var metadataService = context.HttpContext.RequestServices.GetRequiredService<IProtectedResourceMetadataService>();
                try
                {
                    var metadata = await metadataService.GetMetadataAsync(context.HttpContext);
                    var url = metadata.Resource + context.HttpContext.Request.PathBase + "/.well-known/oauth-protected-resource";
                    context.Response.Headers.Append("WWW-Authenticate",
                        $"Bearer resource_metadata=\"{UrlEncoder.Default.Encode(url)}\"");
                }
                catch
                {
                    // ignore metadata errors
                }

                if (originalOnChallenge != null)
                    await originalOnChallenge(context);
            };
        });

        return builder;
    }
}