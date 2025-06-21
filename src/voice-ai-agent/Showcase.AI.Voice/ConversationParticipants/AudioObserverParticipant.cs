using Microsoft.Extensions.Logging;

namespace Showcase.AI.Voice.ConversationParticipants;

public class AudioObserverParticipant : ConversationParticipant
{

    internal Task InternalEventProcessing { get; private set; } = Task.CompletedTask;

    public AudioObserverParticipant(string? id, string? name, ILogger? logger = null) : base(id, name, logger)
    {
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        InternalEventProcessing = Task.Run(() => ProcessInboundEventsAsync(_cts.Token), _cts.Token);
        await Task.WhenAll(InternalEventProcessing).ConfigureAwait(false);
    }

    private async Task ProcessInboundEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var internalEvent in _inboundChannel.Reader.ReadAllAsync(cancellationToken))
            {
                if (internalEvent is RealtimeAudioDeltaEvent audioEvent)
                    await HandleAudioAsync(audioEvent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound events: {Message}", ex.Message);
        }
    }

    private Task HandleAudioAsync(RealtimeAudioDeltaEvent audioEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
