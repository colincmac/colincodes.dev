using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;
public interface IProtectedResourceIssuer
{

    Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken);

    Task<string> GetSignedProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken);
}
