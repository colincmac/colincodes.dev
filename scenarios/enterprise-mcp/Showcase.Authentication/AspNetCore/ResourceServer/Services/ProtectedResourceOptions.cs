using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;
/// <summary>
/// Configuration options for fetching and caching protected resource metadata.
/// </summary>
public sealed class ProtectedResourceOptions
{
    /// <summary>
    /// The protected resource's resource identifier, which is a URL that uses the https scheme and has no fragment component.
    /// </summary>
    /// <remarks>
    /// REQUIRED. The protected resource's resource identifier.
    /// </remarks>
    public required Uri ResourceHostUri { get; init; }

    public string OAuthProtectedResourceRoute { get; set; } = ProtectedResourceConstants.DefaultOAuthProtectedResourceRoute;

    public string JsonWebKeySetEndpointRoute { get; set; } = ProtectedResourceConstants.JsonWebKeySetRoute;

    public ProtectedResourceMetadata? ProtectedResourceMetadata { get; set; }

    #region Signed Protected Resource Metadata Section
    public string? SigningKeyVaultUri { get; set; }

    public string? SigningKeyName { get; set; }
    #endregion
}
