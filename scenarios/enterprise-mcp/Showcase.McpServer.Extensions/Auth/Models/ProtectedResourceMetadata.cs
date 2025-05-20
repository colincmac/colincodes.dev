using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth.Models;
/// <summary>
/// Represents OAuth 2.0 Protected Resource Metadata (RFC9728).
/// </summary>
public class ProtectedResourceMetadata
{
    [JsonPropertyName("resource")] public string Resource { get; set; } = default!;
    [JsonPropertyName("authorization_servers")] public List<string> AuthorizationServers { get; set; } = [];
    [JsonPropertyName("jwks_uri")] public string? JwksUri { get; set; }
    [JsonPropertyName("scopes_supported")] public List<string> ScopesSupported { get; set; } = [];
    [JsonPropertyName("bearer_methods_supported")] public List<string>? BearerMethodsSupported { get; set; } = ["header"];
    [JsonPropertyName("resource_signing_alg_values_supported")] public List<string>? SigningAlgorithms { get; set; }
    // Add other properties as needed following the spec
}
