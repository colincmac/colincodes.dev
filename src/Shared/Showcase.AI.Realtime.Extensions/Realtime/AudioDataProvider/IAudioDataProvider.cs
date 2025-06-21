namespace Showcase.AI.Realtime.Extensions.Realtime.AudioDataProvider;
public interface IAudioDataProvider
{
    Task<ReadOnlyMemory<byte>?> ReadNextChunkAsync(CancellationToken cancellationToken);
}
