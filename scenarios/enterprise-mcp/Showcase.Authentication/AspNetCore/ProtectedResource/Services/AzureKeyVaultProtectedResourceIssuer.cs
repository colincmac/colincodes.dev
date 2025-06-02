using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JwtConstants = Microsoft.IdentityModel.JsonWebTokens.JwtConstants;
using JwtHeaderParameterNames = Microsoft.IdentityModel.JsonWebTokens.JwtHeaderParameterNames;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Services;
public class AzureKeyVaultProtectedResourceIssuer : IProtectedResourceIssuer
{
    private readonly KeyClient _keyClient;
    private readonly CryptographyClient _cryptographyClient;
    private readonly string _keyName;
    private readonly string? _keyVersion;
    private JsonWebKeySet? _jwksDocument;
    private SigningCredentials? _signingCredentials;
    private DateTimeOffset? keyExpiration;
    private JwtSecurityToken? _jwtSecurityToken;

    public AzureKeyVaultProtectedResourceIssuer(KeyClient keyClient, string keyName, string? keyVersion = null)
    {
        _keyClient = keyClient;
        _cryptographyClient = _keyClient.GetCryptographyClient(keyName, keyVersion);
        _keyName = keyName;
        _keyVersion = keyVersion;
    }

    public async Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken = default)
    {
        if (_jwksDocument is not null && _jwksDocument.Keys.Any()) return _jwksDocument;
        KeyVaultKey key = await _keyClient.GetKeyAsync(_keyName, _keyVersion, cancellationToken);
        
        if (key is null || key.Properties.ExpiresOn < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException($"Key '{_keyName}' is expired or not found in Key Vault {_keyClient.VaultUri}.");
        }


        var rsa = key.Key.ToRSA();
        _signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa));
        _jwksDocument = new JsonWebKeySet(JsonSerializer.Serialize(new[] { jwk }));
        return _jwksDocument;
    }

    public async Task<string> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken = default)
    {

        if(_signingCredentials is null)
        {
            await GetJwksDocumentAsync(cancellationToken);
        }

        var metadataResource = metadata.Resource.ToString();
        var jsonPayload = JsonSerializer.SerializeToDocument(metadata) ?? throw new InvalidOperationException("Metadata payload cannot be null.");
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor
            {
                Issuer = metadataResource,
                Audience = null, // Audience can be set if needed
                NotBefore = DateTime.UtcNow,
                Expires = keyExpiration?.UtcDateTime ?? DateTime.UtcNow.AddHours(1), // Default expiration if not set
                IssuedAt = DateTime.UtcNow,
                TokenType = JwtConstants.TokenType,
                SigningCredentials = _signingCredentials,
                Claims = jsonPayload.RootElement.EnumerateObject().ToDictionary(c => c.Name, c => (object)c.Value.ToString()),
        });

        //foreach (var claim in jsonPayload.RootElement.EnumerateObject())
        //{
        //    securityToken.Payload.Add(claim.Name, claim.Value);
        //}


        var unsignedTokenData = securityToken.EncodedHeader + "." + securityToken.EncodedPayload;
        
        var signResult = await _cryptographyClient.SignDataAsync(SignatureAlgorithm.RS256, Encoding.UTF8.GetBytes(unsignedTokenData), cancellationToken: cancellationToken);
        var signature = signResult.Signature;
        return $"{unsignedTokenData}.{Base64UrlEncoder.Encode(signature)}";
    }

}
