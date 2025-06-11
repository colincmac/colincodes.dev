// See https://aka.ms/new-console-template for more information
//using Microsoft.Identity.Client;
//using ModelContextProtocol.Client;
//using ModelContextProtocol.Protocol.Transport;
//using TestApp;

//Console.WriteLine("Protected MCP Weather Server");
//Console.WriteLine();

//var serverUrl = "http://localhost:7071/sse";

//// We can customize a shared HttpClient with a custom handler if desired
//var sharedHandler = new SocketsHttpHandler
//{
//    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
//    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
//};

//var httpClient = new HttpClient(sharedHandler);

//// Create the token provider with our custom HttpClient, 
//// letting the AuthorizationHelpers be created automatically
//var tokenProvider = new OAuthCredentialProvider(
//    new Uri(serverUrl),
//    httpClient,
//    null, // AuthorizationHelpers will be created automatically
//    clientId: "6ad97b5f-7a7b-413f-8603-7a3517d4adb8",
//    redirectUri: new Uri("http://localhost:1179/callback"),
//    scopes: ["api://167b4284-3f92-4436-92ed-38b38f83ae08/weather.read"]
//);

//Console.WriteLine();
//Console.WriteLine($"Connecting to weather server at {serverUrl}...");

//try
//{
//    var transportOptions = new SseClientTransportOptions
//    {
//        Endpoint = new Uri(serverUrl),
//        Name = "Secure Weather Client"
//    };

//    // Create a transport with authentication support using the correct constructor parameters
//    var transport = new SecureSseClientTransport(
//        transportOptions,
//        tokenProvider
//    );
//    var client = await McpClientFactory.CreateAsync(transport);

//    var tools = await client.ListToolsAsync();
//    if (tools.Count == 0)
//    {
//        Console.WriteLine("No tools available on the server.");
//        return;
//    }

//    Console.WriteLine($"Found {tools.Count} tools on the server.");
//    Console.WriteLine();

//    if (tools.Any(t => t.Name == "GetAlerts"))
//    {
//        Console.WriteLine("Calling GetAlerts tool...");

//        var result = await client.CallToolAsync(
//            "GetAlerts",
//            new Dictionary<string, object?> { { "state", "WA" } }
//        );

//        Console.WriteLine("Result: " + result.Content[0].Text);
//        Console.WriteLine();
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"Error: {ex.Message}");
//    if (ex.InnerException != null)
//    {
//        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
//    }

//#if DEBUG
//    Console.WriteLine($"Stack trace: {ex.StackTrace}");
//#endif
//}

using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

var keyClient = new KeyClient(new Uri("https://wdg-mcp-eastus2.vault.azure.net/"), new DefaultAzureCredential());

var cryptoClient = keyClient.GetCryptographyClient("mcp-protected-resource");

var key = await keyClient.GetKeyAsync("mcp-protected-resource");

var alg = SecurityAlgorithms.RsaSha256;
SigningCredentials t = new SigningCredentials(
    new RsaSecurityKey(key.Value.Key.ToRSA(includePrivateParameters: false)),
    alg
);

var metadata = new ProtectedResourceMetadata
{
    Resource = new Uri("https://example.com"),
    AuthorizationServers = new List<Uri>
    {
        new Uri("https://auth.example.com")
    },
    BearerMethodsSupported = new List<string> { "header", "body", "query" },
    ScopesSupported = new List<string> { "read", "write" },
    JwksUri = new Uri("https://example.com/.well-known/jwks.json"),
    ResourceSigningAlgValuesSupported = new List<string> { alg },
    ResourceName = "Example Protected Resource",
    ResourceDocumentation = new Uri("https://example.com/docs"),
    ResourcePolicyUri = new Uri("https://example.com/policy"),
    ResourceTosUri = new Uri("https://example.com/tos"),
};

var jsonPayload = JsonSerializer.SerializeToDocument(metadata) ?? throw new InvalidOperationException("Metadata payload cannot be null.");


var header = new JwtHeader
        {
            { JwtHeaderParameterNames.Typ, JwtConstants.HeaderType },
            { JwtHeaderParameterNames.Alg, alg }
        };
var payload = new JwtPayload(alg, metadata.Resource?.ToString(), null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), DateTime.UtcNow);

header.Base64UrlEncode();
var tokenHandler = new JwtSecurityTokenHandler();
var securityToken = tokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor
{
    Issuer = metadata.Resource?.ToString(), // Issuer can be set if needed
    Audience = null, // Audience can be set if needed
    NotBefore = DateTime.UtcNow,
    Expires = DateTime.UtcNow.AddDays(1), // Default expiration if not set
    IssuedAt = DateTime.UtcNow,
    TokenType = JwtConstants.TokenType,
    SigningCredentials = null,
    Claims = jsonPayload.RootElement.EnumerateObject().ToDictionary(c => c.Name, c => (object)c.Value.ToString()),
});

var unsignedTokenData = header.Base64UrlEncode() + "." + securityToken.EncodedPayload;
var signResult = await cryptoClient.SignDataAsync(alg, Encoding.UTF8.GetBytes(unsignedTokenData));
var tokenValue = unsignedTokenData + "." + Base64UrlEncoder.Encode(signResult.Signature);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


public class ProtectedResourceMetadata
{

    /// <summary>
    /// The resource URI.
    /// </summary>
    /// <remarks>
    /// REQUIRED. The protected resource's resource identifier.
    /// </remarks>
    [JsonPropertyName("resource")]
    public Uri? Resource { get; set; }

    /// <summary>
    /// The list of authorization server URIs.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. JSON array containing a list of OAuth authorization server issuer identifiers
    /// for authorization servers that can be used with this protected resource.
    /// </remarks>
    [JsonPropertyName("authorization_servers")]
    public List<Uri>? AuthorizationServers { get; set; }

    /// <summary>
    /// The supported bearer token methods.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. JSON array containing a list of the supported methods of sending an OAuth 2.0 bearer token
    /// to the protected resource. Defined values are ["header", "body", "query"]. Default value is ["header"].
    /// </remarks>
    [JsonPropertyName("bearer_methods_supported")]
    public List<string>? BearerMethodsSupported { get; set; }

    /// <summary>
    /// The supported scopes.
    /// </summary>
    /// <remarks>
    /// RECOMMENDED. JSON array containing a list of scope values that are used in authorization
    /// requests to request access to this protected resource.
    /// </remarks>
    [JsonPropertyName("scopes_supported")]
    public List<string>? ScopesSupported { get; set; }

    /// <summary>
    /// URL of the protected resource's JSON Web Key (JWK) Set document.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. This contains public keys belonging to the protected resource, such as signing key(s)
    /// that the resource server uses to sign resource responses. This URL MUST use the https scheme.
    /// </remarks>
    [JsonPropertyName("jwks_uri")]
    public Uri? JwksUri { get; set; }

    /// <summary>
    /// List of the JWS signing algorithms supported by the protected resource for signing resource responses.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. JSON array containing a list of the JWS signing algorithms (alg values) supported by the protected resource
    /// for signing resource responses. No default algorithms are implied if this entry is omitted. The value none MUST NOT be used.
    /// </remarks>
    [JsonPropertyName("resource_signing_alg_values_supported")]
    public List<string>? ResourceSigningAlgValuesSupported { get; set; }

    /// <summary>
    /// Human-readable name of the protected resource intended for display to the end user.
    /// </summary>
    /// <remarks>
    /// RECOMMENDED. It is recommended that protected resource metadata include this field.
    /// The value of this field MAY be internationalized.
    /// </remarks>
    [JsonPropertyName("resource_name")]
    public string? ResourceName { get; set; }

    /// <summary>
    /// The URI to the resource documentation.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. URL of a page containing human-readable information that developers might want or need to know
    /// when using the protected resource.
    /// </remarks>
    [JsonPropertyName("resource_documentation")]
    public Uri? ResourceDocumentation { get; set; }

    /// <summary>
    /// URL of a page containing human-readable information about the protected resource's requirements.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. Information about how the client can use the data provided by the protected resource.
    /// </remarks>
    [JsonPropertyName("resource_policy_uri")]
    public Uri? ResourcePolicyUri { get; set; }

    /// <summary>
    /// URL of a page containing human-readable information about the protected resource's terms of service.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. The value of this field MAY be internationalized.
    /// </remarks>
    [JsonPropertyName("resource_tos_uri")]
    public Uri? ResourceTosUri { get; set; }

    /// <summary>
    /// Boolean value indicating protected resource support for mutual-TLS client certificate-bound access tokens.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. If omitted, the default value is false.
    /// </remarks>
    [JsonPropertyName("tls_client_certificate_bound_access_tokens")]
    public bool? TlsClientCertificateBoundAccessTokens { get; set; } = false;

    /// <summary>
    /// List of the authorization details type values supported by the resource server.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. JSON array containing a list of the authorization details type values supported by the resource server
    /// when the authorization_details request parameter is used.
    /// </remarks>
    [JsonPropertyName("authorization_details_types_supported")]
    public List<string>? AuthorizationDetailsTypesSupported { get; set; }

    /// <summary>
    /// List of the JWS algorithm values supported by the resource server for validating DPoP proof JWTs.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. JSON array containing a list of the JWS alg values supported by the resource server
    /// for validating Demonstrating Proof of Possession (DPoP) proof JWTs.
    /// </remarks>
    [JsonPropertyName("dpop_signing_alg_values_supported")]
    public List<string>? DpopSigningAlgValuesSupported { get; set; }

    /// <summary>
    /// Boolean value specifying whether the protected resource always requires the use of DPoP-bound access tokens.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. If omitted, the default value is false.
    /// </remarks>
    [JsonPropertyName("dpop_bound_access_tokens_required")]
    public bool? DpopBoundAccessTokensRequired { get; set; }

    /// <summary>
    /// A JWT containing metadata parameters about the protected resource as claims. This is a string value consisting of the entire signed
    /// JWT. A signed_metadata parameter SHOULD NOT appear as a claim in the JWT; it is RECOMMENDED to reject any metadata in which this occurs.
    /// </summary>
    /// <remarks>
    /// OPTIONAL. Contains signed metadata related to the protected resource.
    /// </remarks>
    [JsonPropertyName("signed_metadata")]
    public string? SignedMetadata { get; set; }
}