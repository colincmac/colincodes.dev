using Microsoft.IdentityModel.Tokens;
using Showcase.Authentication.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Services;
public interface IProtectedResourceIssuer
{

    Task<JsonWebKeySet> GetJwksDocumentAsync(CancellationToken cancellationToken);

    Task<JwtSecurityToken> SignProtectedMetadataAsync(ProtectedResourceMetadata metadata, CancellationToken cancellationToken);
}
