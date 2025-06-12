using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public interface ISignedProtectedResourceIssuer
{

    Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken = default);

    Task<ProtectedResourceMetadata> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken = default);
}
