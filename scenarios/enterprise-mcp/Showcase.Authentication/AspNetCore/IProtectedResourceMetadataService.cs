using Microsoft.AspNetCore.Http;
using Showcase.Authentication.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore;
/// <summary>
/// Fetches and caches protected resource metadata from a well-known endpoint.
/// </summary>
public interface IProtectedResourceMetadataService
{
    Task<ProtectedResourceMetadata> GetMetadataAsync(HttpContext context);
}
