using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;


namespace Showcase.Authentication.AspNetCore.ResourceServer.Services;
internal sealed class ConfigureJwtBearerOptions(ProtectedResourceJwtBearerEvents bearerEvents) : IPostConfigureOptions<JwtBearerOptions>
{
    public string? Scheme { get; set; }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        if (Scheme == name)
        {
            options.Events ??= new JwtBearerEvents();
            options.Events.OnChallenge = CreateChallengeCallback(options.Events.OnChallenge, bearerEvents);

        }
    }

    private Func<JwtBearerChallengeContext, Task> CreateChallengeCallback(Func<JwtBearerChallengeContext, Task> inner, ProtectedResourceJwtBearerEvents bearerEvents)
    {
        async Task Callback(JwtBearerChallengeContext ctx)
        {
            await inner(ctx);
            await bearerEvents.Challenge(ctx);
        }
        return Callback;
    }
}
