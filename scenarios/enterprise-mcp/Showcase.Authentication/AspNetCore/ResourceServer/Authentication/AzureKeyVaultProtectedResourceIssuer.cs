using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Showcase;
using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using JwtConstants = Microsoft.IdentityModel.JsonWebTokens.JwtConstants;
using JwtHeaderParameterNames = Microsoft.IdentityModel.JsonWebTokens.JwtHeaderParameterNames;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public class AzureKeyVaultProtectedResourceIssuer : ISignedProtectedResourceIssuer
{
    private readonly KeyClient _keyClient;
    private readonly CryptographyClient _cryptographyClient;
    private JsonWebKeySet? _jwksDocument;
    private DateTimeOffset keyExpiration = DateTimeOffset.UtcNow;

    private readonly string _keyName;
    private readonly string? _keyVersion = null;

    public AzureKeyVaultProtectedResourceIssuer(IAzureClientFactory<KeyClient> keyClientFactory, IOptionsMonitor<ProtectedResourceOptions> optionsMonitor, [ServiceKey] string serviceKeyName)
    {
        var options = optionsMonitor.GetKeyedOrCurrent(serviceKeyName);
        _keyClient = keyClientFactory.CreateClient(serviceKeyName);
        _cryptographyClient = _keyClient.GetCryptographyClient(options.SigningKeyName, options.SigningKeyObjectVersion);
        _keyName = options.SigningKeyName ?? throw new ArgumentNullException(nameof(options.SigningKeyName), "Signing key name must be provided for Azure Key Vault issuer.");
        _keyVersion = options.SigningKeyObjectVersion; // Optional, can be null if the latest version is desired
    }
    public async Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken = default)
    {
        if (DateTimeOffset.UtcNow < keyExpiration && _jwksDocument is not null && _jwksDocument.Keys.Any()) return _jwksDocument;

        KeyVaultKey keyVaultKey = await _keyClient.GetKeyAsync(_keyName, _keyVersion, cancellationToken);


        if (keyVaultKey is null || keyVaultKey.Properties.ExpiresOn < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException($"Key '{_keyName}' is expired or not found in Key Vault {_keyClient.VaultUri}.");
        }

        keyExpiration = keyVaultKey.Properties.ExpiresOn ?? DateTimeOffset.UtcNow.AddDays(1);

        //var rsa = keyVaultKey.Key.ToRSA(includePrivateParameters: false);
        //var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa));

        _jwksDocument = new JsonWebKeySet(JsonSerializer.Serialize(new[] { keyVaultKey.Key }));

        return _jwksDocument;
    }

    public async Task<ProtectedResourceMetadata> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken = default)
    {
        var certClient = new CertificateClient(_keyClient.VaultUri, new DefaultAzureCredential());
        var cert = certClient.GetCertificate("");
        cert.Value.
        var metadataResource = metadata.Resource?.ToString();
        var jsonPayload = JsonSerializer.SerializeToDocument(metadata) ?? throw new InvalidOperationException("Metadata payload cannot be null.");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = _jwksDocument.Keys.First();
        key.
        var header = new JwtHeader
        {
            { JwtHeaderParameterNames.Typ, JwtConstants.HeaderType },
            { JwtHeaderParameterNames.Alg, _signingCredentials.Algorithm }
        };
        var securityToken = tokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor
            {
                Issuer = metadataResource,
                Audience = null, // Audience can be set if needed
                NotBefore = DateTime.UtcNow,
                Expires = keyExpiration?.UtcDateTime ?? DateTime.UtcNow.AddDays(1), // Default expiration if not set
                IssuedAt = DateTime.UtcNow,
                TokenType = JwtConstants.TokenType,
                SigningCredentials = _signingCredentials,
                Claims = jsonPayload.RootElement.EnumerateObject().ToDictionary(c => c.Name, c => (object)c.Value.ToString()),
        });

        var unsignedTokenData = securityToken.EncodedHeader + "." + securityToken.EncodedPayload;
        
        var signResult = await _cryptographyClient.SignDataAsync(SignatureAlgorithm.RS256, Encoding.UTF8.GetBytes(unsignedTokenData), cancellationToken: cancellationToken);
        var signature = signResult.Signature;
        return $"{unsignedTokenData}.{Base64UrlEncoder.Encode(signature)}";
    }

    public Task<string> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }
}
