namespace Showcase.AI.Voice.ProcessAgent.Configuration;

public class VoiceRagOptions
{
    public const string SectionName = "VoiceRag";
    public string AcsConnectionString { get; set; } = string.Empty;
    public string RealtimeOpenAIDeploymentModelName { get; set; } = string.Empty;
    public string? RealtimeSystemPrompt { get; set; }

    public string ChatOpenAIDeploymentModelName { get; set; } = string.Empty;
    public string? ChatSystemPrompt { get; set; }

    public string CallBackUrl { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
}
