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
using Showcase.Authentication.AspNetCore.Models;
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
    private readonly ProtectedResourceOptions _options;
    private readonly ProtectedResourceMetadata _metadata;
    private readonly string? _hostedResource;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IProtectedResourceIssuer _protectedResourceIssuer;

    public ProtectedResourceMetadataService(
        IOptionsMonitor<ProtectedResourceOptions> optionsMonitor,
        IOptionsMonitor<ProtectedResourceMetadata> metadataMonitor,
        IProtectedResourceIssuer protectedResourceIssuer,
        [ServiceKey] string? hostedResource = null) 
    {
        _options = optionsMonitor.GetKeyedOrCurrent(hostedResource);
        _metadata = metadataMonitor.GetKeyedOrCurrent(hostedResource);
        _hostedResource = hostedResource;
        _protectedResourceIssuer = protectedResourceIssuer;
    }


    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync()
    {
        return Task.FromResult(_metadata);
    }

    public Task<Uri> GetResourceMetadataUriAsync()
    {
        return Task.FromResult($"{hostUri}/{_options.OAuthProtectedResourceRoute}");
    }

    public Task<string> GetWwwAuthenticateHeader(HttpContext context, string? authenticationScheme = JwtBearerDefaults.AuthenticationScheme)
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


