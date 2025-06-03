using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource.Services;
/// <summary>
/// Fetches and caches protected resource metadata from a well-known endpoint.
/// </summary>
public interface IProtectedResourceMetadataProvider
{
    Task<ProtectedResourceMetadata> GetProtectedResourceMetadataAsync(HttpContext context);
    Task<string> GetResourceMetadataUriAsync(HttpContext context);
}
