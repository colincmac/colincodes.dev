using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public sealed class ProtectedResourceService(
    IOptionsMonitor<ProtectedResourceMetadata> metadataMonitor, 
    ISignedProtectedResourceIssuer protectedResourceIssuer, 
    [ServiceKey] string? hostedResource = null) : IProtectedResourceMetadataProvider
{
    public ProtectedResourceMetadata ProtectedResourceMetadata => metadataMonitor.GetKeyedOrCurrent(hostedResource);
    public ProtectedResourceOptions Options => ProtectedResourceMetadata.Options;
    public string UnsignedWwwAuthenticateHeaderValue => $"{ProtectedResourceConstants.WWWAuthenticateKeys.UnsignedResourceMetadata}=\"{ProtectedResourceMetadata.JwksUri}\"";

    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(CancellationToken? cancellationToken = default)
    {
        return Task.FromResult(ProtectedResourceMetadata);
    }

    public Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken? cancellationToken = default)
    {
        return protectedResourceIssuer.GetJwksDocumentAsync(cancellationToken);
    }

    public Task<string> GetSignedProtectedMetadataAsync(CancellationToken? cancellationToken = default)
    {
        return protectedResourceIssuer.GetSignedProtectedMetadataAsync(ProtectedResourceMetadata, cancellationToken);
    }

    public Task<HeaderDictionary> GetWwwAuthenticateHeadersAsync(CancellationToken? cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
