using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestApp;
/// <summary>
/// Represents a token response from the OAuth server.
/// </summary>
internal class TokenContainer
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("ext_expires_in")]
    public int ExtExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the token was obtained.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ObtainedAt { get; set; }

    /// <summary>
    /// Gets the timestamp when the token expires, calculated from ObtainedAt and ExpiresIn.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ExpiresAt => ObtainedAt.AddSeconds(ExpiresIn);
}
