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
    private readonly IEnumerable<ProtectedResourceOptions> _protectedResources;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DiscoveryKeysHealthCheck(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ProtectedResourceOptions> protectedResourceOptions)
    {
        _protectedResources = protectedResourceOptions.;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            var endpoint = _endpoints.FirstOrDefault(x => x.Name == IdentityServerConstants.EndpointNames.Jwks);
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
}