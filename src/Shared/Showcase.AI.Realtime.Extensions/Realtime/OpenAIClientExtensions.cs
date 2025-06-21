using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace Showcase.AI.Realtime.Extensions.Realtime;
public static class OpenAIClientExtensions
{
    public static IVoiceClient AsVoiceClient(this AzureOpenAIClient openAIClient, string modelId, ILogger logger) => new OpenAIVoiceClient(openAIClient, modelId, logger);
}
