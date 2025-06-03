using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Showcase.Authentication.AspNetCore.ProtectedResource.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Showcase.Authentication.AspNetCore.ProtectedResource;
public class DiscoveryKeysHealthCheck : IHealthCheck
{
    private readonly ICollection<Uri> _jwksUris = [];
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DiscoveryKeysHealthCheck(IHttpContextAccessor httpContextAccessor, IEnumerable<NamedService<ProtectedResourceService>> protectedResources, IOptionsMonitor<ProtectedResourceMetadata> protectedResourceMetadataMonitor)
    {
        _httpContextAccessor = httpContextAccessor;

        foreach (var resourceName in protectedResources.Select(x => x.Name))
        {
            var metadata = protectedResourceMetadataMonitor.GetKeyedOrCurrent(resourceName);
            if(metadata?.JwksUri is not null) _jwksUris.Add(metadata.JwksUri);
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            if (endpoint != null)
            {
                if (_httpContextAccessor.HttpContext?.RequestServices.GetRequiredService(endpoint.Handler) is IEndpointHandler handler)
                {
                    var result = await handler.ProcessAsync(_httpContextAccessor.HttpContext);
                    if (result is JsonWebKeysResult)
                    {
                        return HealthCheckResult.Healthy();
                    }
                }
            }
        }
        catch
        {
        }

        return new HealthCheckResult(context.Registration.FailureStatus);
    }

    private IEnumerable<ProtectedResourceMetadata> GetProtectedResources(string[] resourceNames)
    {
        return _protectedResources.Select(x => x.Value).ToList();
    }
}