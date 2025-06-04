using Showcase.Authentication.AspNetCore.ResourceServer;
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
    public string OAuthProtectedResourceRoute { get; set; } = ProtectedResourceConstants.DefaultOAuthProtectedResourceRoute;

    public string JsonWebKeySetEndpointRoute { get; set; } = ProtectedResourceConstants.JsonWebKeySetRoute;

    public string? SigningKeyVaultUri { get; set; }

    public string? SigningKeyName { get; set; }

}
