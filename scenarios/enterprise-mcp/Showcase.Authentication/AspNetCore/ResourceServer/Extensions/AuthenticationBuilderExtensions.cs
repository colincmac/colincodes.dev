using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
using Showcase.Authentication.Core;
using System.Text.Encodings.Web;
using static System.Net.Mime.MediaTypeNames;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Extensions;
public static class AuthenticationBuilderExtensions
{
    
    public static AuthenticationBuilder AddProtectedResourcesToScheme(
        this AuthenticationBuilder builder,
        IConfigurationSection configurationSection,
        string authenticationScheme = JwtBearerDefaults.AuthenticationScheme,
        Action<ProtectedResourceOptions>? configureOptions = null)
    {
        Dictionary<string, ProtectedResourceOptions> options = configurationSection.Get<Dictionary<string, ProtectedResourceOptions>>()
            ?? new Dictionary<string, ProtectedResourceOptions>(StringComparer.OrdinalIgnoreCase);
        foreach (var (schemeName, hostedResourceOptions) in options)
        {
            configureOptions?.Invoke(hostedResourceOptions);
            ArgumentNullException.ThrowIfNull(hostedResourceOptions);
            builder.AddProtectedResourceToScheme(authenticationScheme, (opt) => opt = hostedResourceOptions);
        }
        return builder;
    }

    private static AuthenticationBuilder AddProtectedResourceToScheme(
        this AuthenticationBuilder builder,
        string authenticationScheme = JwtBearerDefaults.AuthenticationScheme,
        Action<ProtectedResourceOptions> configureOptions
        )
    {

        // Configure the authentication scheme's events to include protected resource metadata in the WWW-Authenticate header
        builder.Services.AddTransient<ProtectedResourceJwtBearerEvents>();
        builder.Services.AddSingleton<ConfigureJwtBearerOptions>();
        builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(sp =>
        {
            var opt = sp.GetRequiredService<ConfigureJwtBearerOptions>();
            opt.Scheme = authenticationScheme; // We're only updating the JwtBearerOptions for the specified scheme. This allows for specific schemes to provide protected resource metadata.
            return opt;
        });

        builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        // Configure the options for the protected resource. This is either the host itself or a hosted resource on the server
        // e.g. `https://example.com` or `https://example.com/hostedResourceName`.
        // We're not adding additional schemes, just extending an existing one to include protected resource metadata.

        //var hostedResourceOptionsKey = $"{authenticationScheme}-{hostedResource}".TrimEnd('-');

        // Configure Options
        builder.Services.AddOptions<ProtectedResourceOptions>(authenticationScheme)
            .Configure<IOptionsMonitor<JwtBearerOptions>>((protectedResourceOptions, jwtOptionsMonitor) => 
            {
                configureOptions(protectedResourceOptions);
                var jwtOptions = jwtOptionsMonitor.Get(authenticationScheme);
                Uri t = new Uri()
                // Add the schemes authorization servers if not already set
                if (protectedResourceOptions.Metadata.AuthorizationServers.Count == 0 && !string.IsNullOrEmpty(jwtOptions.Authority))
                {
                    protectedResourceOptions.Metadata.AuthorizationServers.Add(new Uri(jwtOptions.Authority));
                }

                // Add the schemes scopes if not already set
                if (protectedResourceOptions.Metadata.ScopesSupported.Count == 0 && jwtOptions.Configuration?.ScopesSupported != null)
                {
                    protectedResourceOptions.Metadata.ScopesSupported.AddRange(jwtOptions.Configuration.ScopesSupported);
                }

                if(protectedResourceOptions.ProtectedMetadataDiscoveryUri.)
                {
                    // Set the resource metadata URI based on the scheme and options
                    protectedResourceOptions.Metadata.ResourceMetadataUri = new Uri($"{jwtOptions.Authority?.TrimEnd('/')}/{protectedResourceOptions.ProtectedMetadataDiscoveryUri.TrimStart('/')}");
                }
                ConfigureMetadataSigningServices(builder.Services, protectedResourceOptions, authenticationScheme);
            });

        return builder;
    }

    #region (Optional) Signed Protected Metadata Section
    private static IServiceCollection ConfigureMetadataSigningServices(IServiceCollection services, ProtectedResourceOptions options, string hostedResource)
    {
        return options.SigningKeyType switch
        {
            ProtectedResourceMetadataSigningKeyType.None => services,
            ProtectedResourceMetadataSigningKeyType.AzureKeyVaultKey => AddAzureKeyVaultSigningKey(services, options, hostedResource),
            ProtectedResourceMetadataSigningKeyType.AzureKeyVaultCertificate => AddAzureKeyVaultSigningCertificate(services, options, hostedResource),
            _ => throw new InvalidOperationException($"Unsupported signing key type: {options.SigningKeyType}. Supported types are: {ProtectedResourceMetadataSigningKeyType.AzureKeyVaultKey}.")
        };
    }

    private static IServiceCollection AddAzureKeyVaultSigningKey(this IServiceCollection services, ProtectedResourceOptions options, string hostedResource)
    {
        if (string.IsNullOrEmpty(options.SigningKeyVaultUri) ||string.IsNullOrEmpty(options.SigningKeyName)) throw new ArgumentException("SigningKeyVaultUri and SigningKeyName must be set for Azure Key Vault signing key.");

        var vaultUri = new Uri(options.SigningKeyVaultUri);
        services.AddAzureClients(builder =>
        {
            builder.AddKeyClient(vaultUri).WithName(hostedResource);
            builder.AddCryptographyClient(vaultUri).WithName(hostedResource);
            builder.UseCredential(options.AzureTokenCredential);

        });
        
        return services;
    }

    private static IServiceCollection AddAzureKeyVaultSigningCertificate(this IServiceCollection services, ProtectedResourceOptions options, string hostedResource)
    {
        if (string.IsNullOrEmpty(options.SigningKeyVaultUri) || string.IsNullOrEmpty(options.SigningCertificateName)) throw new ArgumentException("SigningKeyVaultUri and SigningCertificateName must be set for Azure Key Vault signing certificate.");

        var vaultUri = new Uri(options.SigningKeyVaultUri);
        services.AddAzureClients(builder =>
        {
            builder.AddCertificateClient(vaultUri).WithName(hostedResource);
            builder.AddCryptographyClient(vaultUri).WithName(hostedResource);
            builder.UseCredential(options.AzureTokenCredential);

        });
        services.AddSingleton<ISignedProtectedResourceIssuer, AzureKeyVaultProtectedResourceIssuer>();
        return services;
    }
    #endregion
}
