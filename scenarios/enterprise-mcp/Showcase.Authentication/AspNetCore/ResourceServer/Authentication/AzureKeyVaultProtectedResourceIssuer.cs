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
    private string? _serviceKey;
    private readonly KeyClient _keyClient;
    private KeyVaultKey? _keyVaultKey;
    private JsonWebKeySet? _jwksDocument;
    IOptionsMonitor<ProtectedResourceOptions> _optionsMonitor;

    public AzureKeyVaultProtectedResourceIssuer(IAzureClientFactory<KeyClient> keyClientFactory, IOptionsMonitor<ProtectedResourceOptions> optionsMonitor, [ServiceKey] string serviceKeyName)
    {
        _optionsMonitor = optionsMonitor;
        _serviceKey = serviceKeyName;
        _keyClient = keyClientFactory.CreateClient(serviceKeyName);
    }

    public async Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken = default)
    {
        var keyVaultKey = await GetOrSetKeyVaultKeyAsync(cancellationToken);

        if (keyVaultKey.Properties.ExpiresOn?.UtcDateTime > DateTime.UtcNow 
            && _jwksDocument is not null 
            && _jwksDocument.Keys.Any()) return _jwksDocument;


        //var rsa = keyVaultKey.Key.ToRSA(includePrivateParameters: false);
        //var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(rsa));

        _jwksDocument = new JsonWebKeySet(JsonSerializer.Serialize(new[] { keyVaultKey.Key }));

        return _jwksDocument;
    }

    public async Task<ProtectedResourceMetadata> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata.Resource, "Protected resource metadata cannot be null.");

        var options = _optionsMonitor.GetKeyedOrCurrent(_serviceKey);
        var cryptoClient = _keyClient.GetCryptographyClient(options.SigningKeyName, options.SigningKeyObjectVersion);
        var key = await GetOrSetKeyVaultKeyAsync(cancellationToken);

        var metadataResource = metadata.Resource.ToString();
        var claims = metadata.ToClaims();
        var header = new JwtHeader
        {
            { JwtHeaderParameterNames.Typ, JwtConstants.HeaderType },
            { JwtHeaderParameterNames.Alg, options.SigningAlgorithm }
        };

        var payload = new JwtPayload(
            issuer: metadataResource, 
            audience: metadataResource, 
            claims: claims, 
            notBefore: DateTime.UtcNow, 
            expires: key.Properties.ExpiresOn?.UtcDateTime ?? DateTime.UtcNow.AddDays(1), 
            issuedAt: DateTime.UtcNow);

        var unsignedTokenData = header.Base64UrlEncode() + "." + payload.Base64UrlEncode();
        
        var signResult = await _cryptographyClient.SignDataAsync(options.SigningAlgorithm, Encoding.UTF8.GetBytes(unsignedTokenData), cancellationToken: cancellationToken);
        var tokenValue = unsignedTokenData + "." + Base64UrlEncoder.Encode(signResult.Signature);

        metadata.SignedMetadata = tokenValue;
        return metadata;
    }

    private async Task<KeyVaultKey> GetOrSetKeyVaultKeyAsync(CancellationToken cancellationToken = default)
    {
        if (_keyVaultKey is not null && _keyVaultKey.Properties.ExpiresOn > DateTime.UtcNow) return _keyVaultKey;

        var options = _optionsMonitor.GetKeyedOrCurrent(_serviceKey);

        KeyVaultKey keyVaultKey = await _keyClient.GetKeyAsync(options.SigningKeyName, options.SigningKeyObjectVersion, cancellationToken);
        return _keyVaultKey ??= keyVaultKey;
    }

}
