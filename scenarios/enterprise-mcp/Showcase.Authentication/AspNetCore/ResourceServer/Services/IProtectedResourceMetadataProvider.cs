using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;
/// <summary>
/// Fetches and caches protected resource metadata from a well-known endpoint.
/// </summary>
public interface IProtectedResourceMetadataProvider
{
    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(CancellationToken cancellationToken = default);
    public Task<JsonWebKeySet> TryGetJwksDocumentAsync(CancellationToken cancellationToken = default);
    public Task<string> GetSignedProtectedMetadataAsync(CancellationToken cancellationToken = default)
}
