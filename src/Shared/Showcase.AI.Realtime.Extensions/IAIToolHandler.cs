using Microsoft.Extensions.AI;

namespace Showcase.AI.Realtime.Extensions;
public interface IAIToolHandler
{
    IEnumerable<AIFunction> GetAITools();
}
