using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.AspNetCore.ProtectedResource.Services;
using System.Text.Encodings.Web;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Extensions;
public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddProtectedResources(
        this AuthenticationBuilder builder,
        IConfigurationSection configurationSection)
    {
        Dictionary<string, ProtectedResourceMetadata> options = configurationSection.Get<Dictionary<string, ProtectedResourceMetadata>>()
            ?? new Dictionary<string, ProtectedResourceMetadata>(StringComparer.OrdinalIgnoreCase);
        builder.Services.AddHttpContextAccessor();
        foreach (var optionsForService in options.Keys)
        {
            builder.Services.Configure<ProtectedResourceMetadata>(optionsForService, configurationSection.GetSection(optionsForService));
            builder.Services.AddSingleton(new NamedService<ProtectedResourceService>(optionsForService));
        }
        return builder;
    }

    private static AuthenticationBuilder AddProtectedResource(
        this AuthenticationBuilder builder,
        ProtectedResourceMetadata metadata)
    {
        string hostedResource = metadata.Resource.AbsolutePath.TrimStart('/').ToLowerInvariant();

        if(!string.IsNullOrEmpty(hostedResource)) builder.Services.AddSingleton(new NamedService<ProtectedResourceService>(hostedResource));

        // Register the signing provider if a signing key is specified
        if (!string.IsNullOrEmpty(metadata.Options.SigningKeyVaultUri) && !string.IsNullOrEmpty(metadata.Options.SigningKeyName))
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
