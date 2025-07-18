namespace Showcase.AI.Realtime.Extensions.Realtime.AudioDataProvider;
public class BinaryDataAudioDataProvider : IAudioDataProvider
{
    private readonly BinaryData _binaryData;
    private int _currentPosition = 0;
    private readonly int _bufferSize;

    public BinaryDataAudioDataProvider(BinaryData binaryData, int bufferSize = 16 * 1024)
    {
        _binaryData = binaryData ?? throw new ArgumentNullException(nameof(binaryData));
        _bufferSize = bufferSize;
    }

    public Task<ReadOnlyMemory<byte>?> ReadNextChunkAsync(CancellationToken cancellationToken)
    {
        var bytes = _binaryData.ToArray();
        var currentLength = bytes.Length;
        if (_currentPosition >= currentLength)
        {
            return Task.FromResult<ReadOnlyMemory<byte>?>(null);
        }

        var bytesToRead = Math.Min(_bufferSize, (int)(bytes.Length - _currentPosition));
        var chunk = new ReadOnlyMemory<byte>(bytes, _currentPosition, bytesToRead);
        _currentPosition += bytesToRead;
        return Task.FromResult<ReadOnlyMemory<byte>?>(chunk);
    }
}
