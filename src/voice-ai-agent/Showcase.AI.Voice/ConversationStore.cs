#pragma warning disable OPENAI002

using OpenAI.RealtimeConversation;
using StackExchange.Redis;
using System.Text.Json;

namespace Showcase.AI.Voice;

public interface IConversationStore
{
    Task AppendConversationHistoryAsync(string conversationId, ConversationUpdate conversationItem);
    Task<IEnumerable<ConversationUpdate>> GetConversationHistoryAsync(string conversationId);
}

public record TranscriptItem(string Role, string Content);

public class RedisConversationStore : IConversationStore
{
    private readonly IDatabase _redis;
    public RedisConversationStore(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }
    public async Task AppendConversationHistoryAsync(string conversationId, ConversationUpdate conversationItem)
    {
        var item = JsonSerializer.Serialize(conversationItem);
        await _redis.ListRightPushAsync(conversationId, item);
    }

    public async Task<IEnumerable<ConversationUpdate>> GetConversationHistoryAsync(string conversationId)
    {
        var entries = await _redis.ListRangeAsync(conversationId, -1);
        return entries
            .Select(entry => JsonSerializer.Deserialize<ConversationUpdate>(entry.ToString()))
            .OfType<ConversationUpdate>();
    }
}
