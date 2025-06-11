using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
/// <summary>
/// Configuration options for fetching and caching protected resource metadata.
/// </summary>
public sealed class ProtectedResourceOptions
{

    //public Uri? ResourceHost { get; set; } = null;
    //public string? HostedResourcePath { get; set; }

    public ProtectedResourceMetadata? Metadata { get; set; }

    public Uri ProtectedMetadataAddress { get; set; } = new Uri(ProtectedResourceConstants.DefaultOAuthProtectedResourcePathSuffix, UriKind.Relative);

    public Uri JwksDocumentAddress { get; set; } = new Uri(ProtectedResourceConstants.JsonWebKeySetPathSuffix, UriKind.Relative);

    public bool RequireHttpsMetadata { get; set; } = true;

    #region Signed Protected Resource Metadata Section
    public string? SigningKeyVaultUri { get; set; }

    public string? SigningKeyName { get; set; }
    public string? SigningCertificateName { get; set; }
    public string? SigningKeyObjectVersion { get; set; }
    public string SigningAlgorithm { get; set; } = SecurityAlgorithms.RsaSha256;

    public TokenCredential AzureTokenCredential { get; set; } = new DefaultAzureCredential();
    public ProtectedResourceMetadataSigningKeyType SigningKeyType
    {
        get
        {
            if (!string.IsNullOrEmpty(SigningKeyVaultUri) && !string.IsNullOrEmpty(SigningKeyName))
            {
                return ProtectedResourceMetadataSigningKeyType.AzureKeyVaultKey;
            }
            else if (!string.IsNullOrEmpty(SigningKeyVaultUri) && !string.IsNullOrEmpty(SigningCertificateName))
            {
                return ProtectedResourceMetadataSigningKeyType.AzureKeyVaultCertificate;
            }
            else
            {
                return ProtectedResourceMetadataSigningKeyType.None;
            }
        }
    }
    #endregion
}

public static class ProtectedResourceOptionsExtensions
{
    /// <summary>
    /// Validates the options for protected resource metadata.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns>True if the options are valid, otherwise false.</returns>
    //public static Uri? TryGetDiscoveryUri(this ProtectedResourceOptions options)
    //{
    //    var discoveryUri = options switch
    //    {
    //        { ProtectedMetadataAddress.IsAbsoluteUri: true } => options.ProtectedMetadataAddress, // If the path is absolute, use it directly

    //        // Try to get the metadata document URI from defined resource host and optional hosted resource path
    //        // Resource Host needs to be absolute
    //        { ResourceHost: { }, HostedResourcePath: null } => new Uri(options.ResourceHost, options.ProtectedMetadataAddress),
    //        { ResourceHost: { } } => new Uri(options.ResourceHost, $"{options.ProtectedMetadataAddress}/{options.HostedResourcePath}"), // If the resource host is absolute and the hosted resource path is provided, combine them
    //        _ => null // If neither is set, return null
    //    };
    //    return discoveryUri;
    //}
}

public class ProtectedResource
{


    /// <summary>
    /// Static protected resource metadata for the resource. 
    /// </summary>
    /// <remarks>
    /// OPTIONAL. If not set, the metadata will be generated from the options and authentication scheme information.
    /// </remarks>
    public ProtectedResourceMetadata? ProtectedResourceMetadata { get; set; }
};
public enum ProtectedResourceMetadataSigningKeyType
{
    None,
    AzureKeyVaultKey,
    AzureKeyVaultCertificate 
}