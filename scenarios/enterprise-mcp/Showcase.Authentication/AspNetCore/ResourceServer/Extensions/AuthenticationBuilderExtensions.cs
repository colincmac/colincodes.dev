using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.AspNetCore.ResourceServer.Services;
using Showcase.Authentication.Core;
using System.Text.Encodings.Web;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Extensions;
public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddProtectedResourcesToScheme(
        this AuthenticationBuilder builder,
        IConfigurationSection configurationSection,
        string authenticationScheme = JwtBearerDefaults.AuthenticationScheme)
    {
        Dictionary<string, ProtectedResourceOptions> options = configurationSection.Get<Dictionary<string, ProtectedResourceOptions>>()
            ?? new Dictionary<string, ProtectedResourceOptions>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in options)
        {
            builder.AddProtectedResourceToScheme(value, authenticationScheme, key);
        }
        return builder;
    }

    private static AuthenticationBuilder AddProtectedResourceToScheme(
        this AuthenticationBuilder builder,
        ProtectedResourceOptions options,
        string authenticationScheme = JwtBearerDefaults.AuthenticationScheme,
        string? hostedResource = null)
    {
        // RFC 9728 allows for hosting multiple resources on a single server. e.g., `/.well-known/oauth-protected-resource/hostedResourceName` and `https://example.com/hostedResourceName`.
        // Here we check to see if the resource is the default resource (`https://example.com`) or a hosted resource (`https://example.com/hostedResourceName`).
        if (!string.IsNullOrEmpty(hostedResource)) builder.Services.AddSingleton(new NamedService<ProtectedResourceService>(hostedResource));

        var protectedResourceMetadata = BuildProtectedResourceMetadataForScheme(authenticationScheme);

        // Register the signing provider if a signing key is specified
        if (!string.IsNullOrEmpty(options.SigningKeyVaultUri) && !string.IsNullOrEmpty(options.SigningKeyName))
        {
            //builder.Services.AddKeyedScoped<IProtectedResourceIssuer, AzureKeyVaultProtectedResourceIssuer>(hostedResource, (sp) =>
            //{
            //    var options = sp.GetRequiredService<IOptionsMonitor<ProtectedResourceMetadata>>().Get(hostedResource);
            //    return new AzureKeyVaultProtectedResourceIssuer(options.Options.SigningKeyName, options.Options.SigningKeyVaultUri, sp.GetRequiredService<IHttpContextAccessor>());
            //});
            //builder.Services.AddAzureKeyVaultCertificate(metadata.Options.SigningKeyVaultName, metadata.Options.SigningKeyVaultUri);
        }

        builder.Services.AddSingleton<IProtectedResourceMetadataProvider, ProtectedResourceMetadataProvider>();

        return builder;
    }

    private static ProtectedResourceMetadata BuildProtectedResourceMetadataForScheme(string authenticationScheme)
    {
        // This method can be used to build a ProtectedResourceMetadata instance based on the provided options.
        // It can be customized to include additional properties or logic as needed.
        return new ProtectedResourceMetadata
        {
            Resource = new Uri("https://example.com/.well-known/oauth-protected-resource"),
            AuthorizationServers = new List<Uri> { new Uri("https://auth.example.com") },
            BearerMethodsSupported = new List<string> { "header", "query" },
            ScopesSupported = new List<string> { "read", "write" }
        };
    }

    /// <summary>
    /// Updates the JWTBearer Options to add a protected resource metadata URL into challenge responses.
    /// </summary>
    public static AuthenticationBuilder AddProtectedResource(
        this AuthenticationBuilder builder,
        string? hostedResource = null,
        Action<ProtectedResourceMetadata> configureProtectedResourceMetadata = null)
    {
        builder.Services.AddHttpContextAccessor();

        if (!string.IsNullOrEmpty(hostedResource))
        {
            builder.Services.AddSingleton(new NamedService<ProtectedResourceService>(hostedResource));
        }

        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, (options) => 
        {
            if(configureOptions is not null) configureOptions(options);
            var originalOnChallenge = options.Events.OnChallenge;
            options.Events.OnChallenge += async (context) =>
            {

                var metadataService = context.HttpContext.RequestServices.GetRequiredService<IProtectedResourceMetadataProvider>();
                try
                {
                    var url = $"{context.HttpContext}";

                    /**
                     * TODO: Investigate whether we need to add additional logic for the WWW-Authenticate header based on the default JWT Bearer handler.
                     * The JWT Bearer handler invokes default handler logic before events, adding it's own `Bearer realm...` context, resulting in a WWW-Authenticate header like `Bearer realm="https", resource_metadata="https://example.com/.well-known/oauth-protected-resource". 
                     */
                    var resourceMetadataString = $"{ ProtectedResourceConstants.WWWAuthenticateKeys.UnsignedResourceMetadata }=\"{UrlEncoder.Default.Encode(url)}\"";
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
