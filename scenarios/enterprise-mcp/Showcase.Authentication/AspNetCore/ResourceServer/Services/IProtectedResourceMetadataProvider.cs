using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;

public interface IProtectedResourceMetadataProvider
{
    public Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(CancellationToken? cancellationToken = default);
    public Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken? cancellationToken = default);
    public Task<string> GetSignedProtectedMetadataAsync(CancellationToken? cancellationToken = default);
    public Task<HeaderDictionary> GetWwwAuthenticateHeadersAsync(CancellationToken? cancellationToken = default);
}
