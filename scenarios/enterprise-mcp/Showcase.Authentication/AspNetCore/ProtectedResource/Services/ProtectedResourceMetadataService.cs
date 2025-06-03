using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Services;

public class ProtectedResourceMetadataService : IProtectedResourceMetadataService
{
    private readonly IOptionsMonitor<ProtectedResourceOptions> _optionsMonitor;
    private readonly IOptionsMonitor<ProtectedResourceMetadata> _metadataMonitor;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IProtectedResourceIssuer _protectedResourceIssuer;

    public ProtectedResourceMetadataService(
        IOptionsMonitor<ProtectedResourceOptions> optionsMonitor,
        IOptionsMonitor<ProtectedResourceMetadata> metadataMonitor,
        IProtectedResourceIssuer protectedResourceIssuer) 
    {
        _optionsMonitor = optionsMonitor;
        _metadataMonitor = metadataMonitor;
        _protectedResourceIssuer = protectedResourceIssuer;
    }


    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(string? hostedResource = null)
    {
        var metadata = _metadataMonitor.GetKeyedOrCurrent(hostedResource);
        if (metadata == null)
        {
            throw new InvalidOperationException($"Protected resource metadata not found for hosted resource '{hostedResource}'.");
        }
        return Task.FromResult(metadata);
    }

    public Task<Uri> GetResourceMetadataUriAsync(string? hostedResource = null)
    {
        var options = _optionsMonitor.GetKeyedOrCurrent(hostedResource);

        if (options == null)
        {
            throw new InvalidOperationException($"Protected resource options not found for hosted resource '{hostedResource}'.");
        }

        return Task.FromResult($"{options.}/{options.OAuthProtectedResourceRoute}");
    }

    public Task<string> GetWwwAuthenticateHeaderAsync(HttpContext context, string? authenticationScheme = JwtBearerDefaults.AuthenticationScheme)
    {        
        var resourceUri = GetResourceUriFromContext(context);
        context.Response.Headers.WWWAuthenticate.Append(authenticationScheme);
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.FromResult($"Bearer realm=\"{resourceUri}\", resource=\"{resourceUri}\"");
    }

    private string GetResourceUriFromContext(HttpContext context)
    {
        var resourceUri = $"{context.Request.Scheme}://{context.Request.Host}";
        if (!string.IsNullOrEmpty(_hostedResource)) resourceUri += $"/{_hostedResource}";
        return resourceUri;
    }


}


