using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;
internal class ProtectedResourceJwtBearerEvents
{
    private readonly IOptionsMonitor<ProtectedResourceOptions> _optionsMonitor;
    private readonly ILogger<ProtectedResourceJwtBearerEvents> _logger;

    public ProtectedResourceJwtBearerEvents(IOptionsMonitor<ProtectedResourceOptions> protectedResourceOptionsMonitor, ILogger<ProtectedResourceJwtBearerEvents> logger)
    {
        _optionsMonitor = protectedResourceOptionsMonitor;
        _logger = logger;
    }

    public Task Challenge(JwtBearerChallengeContext context)
    {
        var protectedResourceOptions = _optionsMonitor.Get(context.Scheme.Name);
        

        _logger.LogInformation("Challenge initiated for scheme: {Scheme}", context.Scheme.Name);
        
        return Task.CompletedTask;
    }
}
