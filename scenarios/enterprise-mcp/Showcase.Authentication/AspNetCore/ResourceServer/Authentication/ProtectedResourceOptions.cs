using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
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
    public ProtectedResourceMetadata Metadata { get; set; } = new() { 
        Resource = new Uri("/", UriKind.Relative)
    };

    public Uri ProtectedMetadataPath { get; set; } = new Uri(ProtectedResourceConstants.DefaultOAuthProtectedResourcePathSuffix, UriKind.Relative);

    public Uri JwksDocumentPath { get; set; } = new Uri(ProtectedResourceConstants.JsonWebKeySetPathSuffix, UriKind.Relative);


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

public enum ProtectedResourceMetadataSigningKeyType
{
    None,
    AzureKeyVaultKey,
    AzureKeyVaultCertificate 
}