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
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using JwtConstants = Microsoft.IdentityModel.JsonWebTokens.JwtConstants;
using JwtHeaderParameterNames = Microsoft.IdentityModel.JsonWebTokens.JwtHeaderParameterNames;

var keyClient = new KeyClient(new Uri("https://wdg-mcp-eastus2.vault.azure.net/"), new DefaultAzureCredential());

var cryptoClient = keyClient.GetCryptographyClient("mcp-protected-resource");

var key = await keyClient.GetKeyAsync("mcp-protected-resource");


var alg = SecurityAlgorithms.RsaSha256;

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

//var claims = JsonSerializer.SerializeToNode(metadata)?.AsObject()
//    .Select(x => new Claim(x.Key, x.Value?.ToString(), JsonClaimValueTypes.Json)) ?? throw new InvalidOperationException("Metadata payload cannot be null.");
var claims = metadata.ToClaims() ?? throw new InvalidOperationException("Metadata payload cannot be null.");

var header = new JwtHeader
        {
            { JwtHeaderParameterNames.Typ, JwtConstants.HeaderType },
            { JwtHeaderParameterNames.Alg, alg }
        };
var payload = new JwtPayload(metadata.Resource?.ToString(), metadata.Resource?.ToString(), claims, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), DateTime.UtcNow);


var unsignedTokenData = header.Base64UrlEncode() + "." + payload.Base64UrlEncode();
var signResult = await cryptoClient.SignDataAsync(alg, Encoding.UTF8.GetBytes(unsignedTokenData));
var tokenValue = unsignedTokenData + "." + Base64UrlEncoder.Encode(signResult.Signature);
var verified = await cryptoClient.VerifyDataAsync(alg, Encoding.UTF8.GetBytes(unsignedTokenData), signResult.Signature);

var tokenHandler = new JsonWebTokenHandler();
var signingKey = new RsaSecurityKey(key.Value.Key.ToRSA())
{
    
};
var valid = await tokenHandler.ValidateTokenAsync(tokenValue, new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new RsaSecurityKey(key.Value.Key.ToRSA(includePrivateParameters: false)),
    ValidateIssuer = true,
    ValidIssuer = metadata.Resource?.ToString() ?? throw new InvalidOperationException("Resource URI must be set."),
    ValidateAudience = false,
    ClockSkew = TimeSpan.Zero
});

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

