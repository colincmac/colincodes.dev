#pragma warning disable OPENAI002

using Microsoft.Extensions.Logging;
using OpenAI.RealtimeConversation;
using Showcase.AI.Realtime.Extensions.Realtime;
using Showcase.AI.Voice.ConversationParticipants;

namespace Showcase.AI.Voice.Agents.RealtimeVoice;

// This should be a Delegating agent that wraps an AI agent that uses raw audio in conversations.
public class RealtimeConversationAgent : OpenAIVoiceParticipant
{
    public RealtimeConversationAgent(
        RealtimeConversationClient aiClient,
        RealtimeSessionOptions sessionOptions,
        ILoggerFactory loggerFactory,
        string id,
        string name) : base(aiClient, sessionOptions, loggerFactory, id, name)
    {
    }


}
