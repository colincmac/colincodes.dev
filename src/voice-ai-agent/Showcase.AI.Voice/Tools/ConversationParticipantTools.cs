using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Showcase.AI.Realtime.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace Showcase.AI.Voice.Tools;

public class ConversationParticipantTools : IAIToolHandler
{
    private readonly ConversationParticipant _invokingParticipant;
    private readonly IList<ConversationParticipant> _knownParticipants;
    private readonly ILogger _logger;

    public ConversationParticipantTools(ConversationParticipant invokingParticipant, IList<ConversationParticipant> knownParticipants, ILogger logger)
    {
        _invokingParticipant = invokingParticipant;
        _knownParticipants = knownParticipants;
        _logger = logger;
    }

    public IEnumerable<AIFunction> GetAITools()
    {
        var tools = GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<AIToolAttribute>() is not null)
            .Select(tool =>
            {
                var attribute = tool.GetCustomAttribute<AIToolAttribute>();

                return AIFunctionFactory.Create(tool, this, options: new()
                {
                    Name = attribute?.Name,
                    Description = attribute?.Description
                });
            });
        foreach (var tool in tools)
        {
            yield return tool;
        }
    }

    [AITool(name: "transferAgent", description: "Triggers a transfer of the user to a more specialized agent. \r\n  Calls escalate to a more specialized LLM agent or to a human agent, with additional context.")]
    public Task TransferToAgentAsync(
        [Description("The reasoning why this transfer is needed.")] string reasonForTransfer,
        [Description("Relevant context from the conversation that will help the recipient perform the correct action.")] string conversationContext,
        [Description("The name of the agent to transfer to.")] string agentName
        ) => Task.FromResult(() =>
        {
            _logger.LogInformation($"Transferring conversation with reason: {reasonForTransfer} \n ConversationContext: {conversationContext} \n TransferringTo: {agentName}");
        });

    [AITool(name: "terminateConversation", description: "Ends the current call")]
    public Task StartTerminatingConversationAsync(
    [Description("The reasoning why the conversation is being terminated.")] string reasonForTermination
    ) => Task.FromResult(() =>
    {
        _logger.LogInformation($"Terminating conversation with reason: {reasonForTermination}");
    });

    public enum ConversationTermination
    {
        Continue,
        EndCallGracefully,
        EscalateToLivePerson,
        WaitForResponse,
    }
}
