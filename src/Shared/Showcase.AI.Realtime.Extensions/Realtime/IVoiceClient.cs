using OpenAI.RealtimeConversation;

namespace Showcase.AI.Realtime.Extensions.Realtime;

#pragma warning disable OPENAI002
public delegate Task ConversationUpdateHandler<T>(IOutputWebSocket output, T update) where T : ConversationUpdate;

public interface IVoiceClient : IAsyncDisposable
{
    Task StartConversationAsync(IOutputWebSocket output, RealtimeSessionOptions sessionOptions, CancellationToken cancellationToken = default);
    Task SendInputAudioAsync(BinaryData audio, CancellationToken cancellationToken = default);
    IVoiceClient OnConversationUpdate<T>(ConversationUpdateHandler<T> handler) where T : ConversationUpdate;
}
