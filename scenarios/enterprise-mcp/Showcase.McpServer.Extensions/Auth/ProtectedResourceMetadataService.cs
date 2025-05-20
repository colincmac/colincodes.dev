using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Showcase.McpServer.Extensions.Auth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth;
public class ProtectedResourceMetadataService : IProtectedResourceMetadataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache _cache;
    private readonly ProtectedResourceMetadataOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private DateTimeOffset _jwksLastRefreshed;
    private JsonWebKeySet? _cachedJwks;

    public ProtectedResourceMetadataService(
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache,
        IOptions<ProtectedResourceMetadataOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<ProtectedResourceMetadata> GetMetadataAsync(HttpContext context)
    {
        var host = $"{context.Request.Scheme}://{context.Request.Host}";
        var endpoint = host + _options.WellKnownPath;

        ProtectedResourceMetadata? metadata = default;
        // Use cache key per host
        var cacheKey = $"prm:{host}";
        var cacheBytes = await _cache.GetAsync(cacheKey, context.RequestAborted);
        
        if (cacheBytes is not null)
        {
            metadata = JsonSerializer.Deserialize<ProtectedResourceMetadata>(cacheBytes, _jsonOptions)
                   ?? throw new InvalidOperationException("Failed to deserialize protected resource metadata from cache.");
        }
        else
        {
            var client = _httpClientFactory.CreateClient();
            var jws = await client.GetStringAsync(endpoint, context.RequestAborted);
            
            // Lazy-refresh JWKS
            if (_cachedJwks == null || DateTimeOffset.UtcNow - _jwksLastRefreshed > _options.JwksRefreshInterval)
            {
                // Preliminary fetch to get jwks_uri (unsigned)
                var unsigned = await GetUnsignedMetadataAsync(client, host);
                if (string.IsNullOrEmpty(unsigned.JwksUri))
                    throw new InvalidOperationException("jwks_uri not provided in metadata.");

                var jwksJson = await client.GetStringAsync(unsigned.JwksUri, context.RequestAborted);
                _cachedJwks = new JsonWebKeySet(jwksJson);
                _jwksLastRefreshed = DateTimeOffset.UtcNow;
            }

            // Validate JWS signature
            var validToken = ValidateJwsSignature(jws, _cachedJwks, host);

            // Deserialize payload
            var payloadJson = JsonSerializer.Serialize(jwt.Payload);
            metadata = JsonSerializer.Deserialize<ProtectedResourceMetadata>(payloadJson, _jsonOptions)
                       ?? throw new InvalidOperationException("Failed to deserialize protected resource metadata.");

            // Validate resource claim
            if (!string.Equals(metadata.Resource, host, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Metadata resource identifier mismatch.");

            var response = await client.GetAsync(endpoint, context.RequestAborted);
            response.EnsureSuccessStatusCode();

            // Read the response content as a byte array once
            var bytes = await response.Content.ReadAsByteArrayAsync(context.RequestAborted);

            // Write into the cache using the byte array
            await _cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions()
            {
                SlidingExpiration = _options.CacheDuration,
            }, context.RequestAborted);

            metadata = JsonSerializer.Deserialize<ProtectedResourceMetadata>(bytes, _jsonOptions)
                      ?? throw new InvalidOperationException("Failed to deserialize protected resource metadata from endpoint.");
        }

        // Validate resource field matches
        if (!string.Equals(metadata.Resource, host, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Metadata resource identifier mismatch.");

        return metadata;
    }

    private JwtSecurityToken ValidateJwsSignature(string jws, JsonWebKeySet jwks, string host)
    {
        // Validate JWS signature
        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            IssuerSigningKeys = jwks.Keys,
            ValidIssuer = host,
            ValidateIssuer = true,
            ValidateAudience = false,
            RequireSignedTokens = true
        };
        _ = handler.ValidateToken(jws, validationParams, out var token);
        if (token is not JwtSecurityToken validatedSecurityToken)
            throw new SecurityTokenInvalidSignatureException("Invalid JWS signature.");
        return validatedSecurityToken;
    }

    // Helper to fetch unsigned metadata for initial jwks_uri discovery
    private async Task<ProtectedResourceMetadata> GetUnsignedMetadataAsync(HttpClient client, string host)
    {
        var uri = host + _options.WellKnownPath;
        var resp = await client.GetAsync(uri);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync();
        return JsonSerializer.Deserialize<ProtectedResourceMetadata>(stream, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize unsigned metadata.");
    }
}


