using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;

public interface IProtectedResourceMetadataProvider
{
    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(Uri? resourceUri = null, CancellationToken? cancellationToken = default);
    public Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken? cancellationToken = default);
}
