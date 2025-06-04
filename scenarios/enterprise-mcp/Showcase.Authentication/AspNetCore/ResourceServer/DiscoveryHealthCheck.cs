using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Showcase.Authentication.AspNetCore.ResourceServer.Services;


namespace Showcase.Authentication.AspNetCore.ResourceServer;
public class DiscoveryKeysHealthCheck : IHealthCheck
{
    private readonly ICollection<Uri> _jwksUris = [];
    private readonly IHttpClientFactory _httpClientFactory;

    public DiscoveryKeysHealthCheck(
        IEnumerable<NamedService<ProtectedResourceService>> protectedResources,
        IOptionsMonitor<ProtectedResourceMetadata> protectedResourceMetadataMonitor,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        foreach (var resourceName in protectedResources.Select(x => x.Name))
        {
            var metadata = protectedResourceMetadataMonitor.GetKeyedOrCurrent(resourceName);
            if (metadata?.JwksUri is not null)
                _jwksUris.Add(metadata.JwksUri);
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_jwksUris.Count == 0)
        {
            return HealthCheckResult.Healthy("No JWKS URIs configured.");
        }

        var client = _httpClientFactory.CreateClient();
        var errors = new List<string>();

        foreach (var jwksUri in _jwksUris)
        {
            try
            {
                using var response = await client.GetAsync(jwksUri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    errors.Add($"JWKS endpoint {jwksUri} returned status {response.StatusCode}.");
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content) || !content.Contains("\"keys\""))
                {
                    errors.Add($"JWKS endpoint {jwksUri} did not return valid JWKS data.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"JWKS endpoint {jwksUri} threw exception: {ex.Message}");
            }
        }

        if (errors.Count == 0)
        {
            return HealthCheckResult.Healthy("All JWKS endpoints are healthy.");
        }

        return new HealthCheckResult(context.Registration.FailureStatus, description: string.Join("; ", errors));
    }
}
