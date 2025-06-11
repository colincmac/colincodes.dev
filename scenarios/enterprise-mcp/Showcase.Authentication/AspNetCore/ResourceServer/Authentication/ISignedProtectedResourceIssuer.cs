using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.Core;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Authentication;
public interface ISignedProtectedResourceIssuer
{

    Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken = default);

    Task<ProtectedResourceMetadata> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken = default);
}
