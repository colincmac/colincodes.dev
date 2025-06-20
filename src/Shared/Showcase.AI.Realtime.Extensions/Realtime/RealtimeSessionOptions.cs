using Microsoft.Extensions.AI;
using OpenAI.RealtimeConversation;
using System.Text.Json.Serialization;

namespace Showcase.AI.Realtime.Extensions.Realtime;

#pragma warning disable OPENAI002
public class RealtimeSessionOptions
{
    public const string ConfigurationSection = "RealtimeSessionOptions";
    public string? AgentName { get; set; }
    public string? ModelId { get; set; }

    public string? Instructions { get; set; }
    public ConversationVoice? Voice { get; set; }
    public ConversationAudioFormat? InputAudioFormat { get; set; }
    public ConversationAudioFormat? OutputAudioFormat { get; set; }

    [JsonIgnore]
    public IList<AITool>? Tools { get; set; }
    public float? Temperature { get; set; }
    public ConversationToolChoice? ToolChoice { get; set; }

    public ConversationMaxTokensChoice? MaxOutputTokens { get; set; }

    public ConversationTurnDetectionOptions? TurnDetectionOptions { get; set; }
    public ConversationInputTranscriptionOptions? InputTranscriptionOptions { get; set; }
    public ConversationContentModalities ContentModalities { get; set; }
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }
}
