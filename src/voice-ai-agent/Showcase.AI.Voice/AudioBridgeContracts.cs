#pragma warning disable OPENAI002

using System.Text.Json.Serialization;

namespace Showcase.AI.Voice;

#region Shared Events


[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(EventType))]
public abstract record RealtimeEvent()
{
    public abstract string EventType { get; }

    public string EventId { get; init; } = Guid.CreateVersion7(DateTimeOffset.UtcNow).ToString();
    public string ServiceEventType { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public string? AuthorName { get; init; } = string.Empty;
    public int OutputIndex { get; init; } = 0;
    public int ContentIndex { get; init; } = 0;
};

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeAudioDeltaEvent))]
public record RealtimeAudioDeltaEvent(BinaryData AudioData, string ConversationRole, string? TranscriptText = null) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeAudioDeltaEvent);
    public bool IsAudioEmpty => AudioData is null;
    public bool IsTranscriptEmpty => string.IsNullOrEmpty(TranscriptText);
};


[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeMessageEvent))]
public record RealtimeMessageEvent(IEnumerable<string> ChatMessageContent, string ConversationRole) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeMessageEvent);
    public bool IsEmpty => !ChatMessageContent.Any();
}

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeTranscriptFinishedEvent))]
public record RealtimeTranscriptFinishedEvent(string Transcription, string ConversationRole) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeTranscriptFinishedEvent);
    public bool IsEmpty => string.IsNullOrWhiteSpace(Transcription);
}

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeMetricDeltaEvent))]
public record RealtimeMetricDeltaEvent(BinaryData Metric) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeMetricDeltaEvent);
    public bool IsEmpty => Metric is null;
}


// Similar to Stop Audio from ACS
[JsonDerivedType(typeof(RealtimeEvent), nameof(ParticipantStartedSpeakingEvent))]
public record ParticipantStartedSpeakingEvent(string ConversationRole) : RealtimeEvent
{
    public override string EventType => nameof(ParticipantStartedSpeakingEvent);
    public bool IsEmpty => false;
}

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeVideoDeltaEvent))]
public record RealtimeVideoDeltaEvent(BinaryData VideoData, string ConversationRole) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeVideoDeltaEvent);
    public bool IsEmpty => VideoData is null;
}

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeUserIntentDiscoveredEvent))]
public record RealtimeUserIntentDiscoveredEvent(string ParticipantId, string Intent) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeUserIntentDiscoveredEvent);
    public bool IsEmpty => true;
}

[JsonDerivedType(typeof(RealtimeEvent), nameof(RealtimeUserIntentFulfilledEvent))]
public record RealtimeUserIntentFulfilledEvent(string ConversationRole, string ParticipantId) : RealtimeEvent
{
    public override string EventType => nameof(RealtimeUserIntentFulfilledEvent);
    public bool IsEmpty => true;
}

#endregion
