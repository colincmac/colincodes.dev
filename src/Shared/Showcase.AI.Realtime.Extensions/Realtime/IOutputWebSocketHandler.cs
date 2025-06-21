namespace Showcase.AI.Realtime.Extensions.Realtime;

public interface IOutputWebSocket : IAsyncDisposable
{
    Task StartProcessingWebSocketAsync(IVoiceClient voiceClient, CancellationToken cancellationToken = default);
    Task SendInputAudioAsync(byte[] audio, CancellationToken token = default);
    BinaryData ConvertAudioToRequestData(byte[] audioIn);
    BinaryData? TryGetAudioFromResponse(byte[] audioOut);
    Task SendStopAudioCommandAsync(CancellationToken cancellationToken = default);
}
