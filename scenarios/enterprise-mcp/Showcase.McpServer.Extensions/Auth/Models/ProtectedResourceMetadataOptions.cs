using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth.Models;
/// <summary>
/// Configuration options for fetching and caching protected resource metadata.
/// </summary>
public class ProtectedResourceMetadataOptions
{
    /// <summary>
    /// Path suffix under the host for the metadata endpoint (default: /.well-known/oauth-protected-resource).
    /// </summary>
    public string WellKnownPath { get; set; } = "/.well-known/oauth-protected-resource";

    /// <summary>
    /// Duration to cache the fetched metadata.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// If true, expects metadata as a signed JWT and validates its signature.
    /// </summary>
    public bool ValidateSignature { get; set; } = false;

    /// <summary>
    /// How often to refresh JWKS from jwks_uri.
    /// </summary>
    public TimeSpan JwksRefreshInterval { get; set; } = TimeSpan.FromHours(24);
}
