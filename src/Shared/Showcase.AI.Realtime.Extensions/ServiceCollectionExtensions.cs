using Microsoft.Extensions.DependencyInjection;
using Showcase.AI.Realtime.Extensions.Realtime;

namespace Showcase.AI.Realtime.Extensions;
public static class ServiceCollectionExtensions
{
    //public static IServiceCollection AddAIToolRegistry(this IServiceCollection services, IEnumerable<AIFunction>? tools = default)
    //{
    //    return services.AddSingleton(new AIToolRegistry(tools ?? []));
    //}

    public static IServiceCollection AddVoiceClient(this IServiceCollection services)
    {
        return services.AddScoped<IVoiceClient, OpenAIVoiceClient>();
    }

}
