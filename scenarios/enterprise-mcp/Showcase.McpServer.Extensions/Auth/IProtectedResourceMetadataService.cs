using Microsoft.AspNetCore.Http;
using Showcase.McpServer.Extensions.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth;
/// <summary>
/// Fetches and caches protected resource metadata from a well-known endpoint.
/// </summary>
public interface IProtectedResourceMetadataService
{
    Task<ProtectedResourceMetadata> GetMetadataAsync(HttpContext context);
}
