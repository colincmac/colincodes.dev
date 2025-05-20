using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Showcase.McpServer.Extensions.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.McpServer.Extensions.Auth;
public static  class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers services for protected resource metadata fetching and caching.
    /// </summary>
    public static IServiceCollection AddProtectedResourceMetadata(this IServiceCollection services,
            IConfiguration configuration, string? configurationSectionName = default)
    {
        var sectionName = configurationSectionName ?? "ProtectedResourceMetadata";
        services.Configure<ProtectedResourceMetadataOptions>(configuration.GetSection(sectionName));

        services.AddHttpClient();
        services.AddDistributedMemoryCache();
        services.AddSingleton<IProtectedResourceMetadataService, ProtectedResourceMetadataService>();
        return services;
    }
}
