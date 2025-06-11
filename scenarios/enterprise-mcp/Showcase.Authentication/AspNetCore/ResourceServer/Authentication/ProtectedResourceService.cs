using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public sealed class ProtectedResourceService : IProtectedResourceMetadataProvider
{
    private readonly IOptionsMonitor<ProtectedResourceOptions> _optionsMonitor;
    private readonly string? _hostedResource;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISignedProtectedResourceIssuer? _protectedResourceIssuer;

    public ProtectedResourceService(
        IOptionsMonitor<ProtectedResourceOptions> optionsMonitor,       
        IHttpContextAccessor httpContextAccessor,
        ISignedProtectedResourceIssuer? protectedResourceIssuer,
        [ServiceKey] string? hostedResource = null)
    {
        _optionsMonitor = optionsMonitor;
        _httpContextAccessor = httpContextAccessor;
        _protectedResourceIssuer = protectedResourceIssuer;
        _hostedResource = hostedResource;
    }

    public async Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(CancellationToken? cancellationToken = default)
    {
        var options = _optionsMonitor.GetKeyedOrCurrent(_hostedResource);
        options.Metadata.Resource ??= _httpContextAccessor.HttpContext.Request.U
        if (options.Metadata.Resource is null && _httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("The Resource Metadata `Resource` value must be set statically or provided from the HTTPContext");
        }

        if (options.Metadata.Resource is null && _httpContextAccessor.HttpContext?.Request.Path is null) 
            

        return Task.FromResult(ProtectedResourceMetadata);
    }
    public Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
}
