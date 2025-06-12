using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
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

