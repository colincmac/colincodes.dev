#pragma warning disable OPENAI002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OpenAI.RealtimeConversation;
using Showcase.AI.Realtime.Extensions;
using Showcase.AI.Realtime.Extensions.Realtime;
using Showcase.AI.Voice;
using Showcase.AI.Voice.ProcessAgent.Configuration;
using Showcase.ServiceDefaults;


AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
//builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



builder.Services.Configure<VoiceRagOptions>(
    builder.Configuration.GetSection(VoiceRagOptions.SectionName));

builder.AddAzureOpenAIClient("openai")
    .AddChatClient(
        deploymentName: builder.Configuration.GetValue<VoiceRagOptions>(VoiceRagOptions.SectionName)?.ChatOpenAIDeploymentModelName
        );

builder.Services.AddScoped(sp =>
{
    var options = sp.GetRequiredService<IOptions<VoiceRagOptions>>().Value;
    return new CallAutomationClient(connectionString: options.AcsConnectionString);
});
builder.Services.AddSingleton<IAIToolRegistry, AIToolRegistry>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapGet("/", () => "Welcome to the Voice AI Agent!");

app.MapPost("/api/callbacks/{contextId}", (
    [FromBody] CloudEvent[] cloudEvents,
    [FromRoute] string contextId,
    [Required] string callerId,
    ILogger<Program> logger) =>
{

    foreach (var cloudEvent in cloudEvents)
    {
        CallAutomationEventBase @event = CallAutomationEventParser.Parse(cloudEvent);
        logger.LogInformation($"Event received: {JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true })}");
    }

    return Results.Ok();
});

app.MapPost("/api/incomingCall", async (
    [FromBody] CloudEvent[] cloudEvents,
    [FromServices] CallAutomationClient client,
    [FromServices] IOptions<VoiceRagOptions> configurationOptions,
    ILogger<Program> logger) =>
{
    foreach (var cloudEvent in cloudEvents)
    {
        Console.WriteLine($"Incoming Call event received.");

        // Handle system events
        if (cloudEvent.TryGetSystemEventData(out object eventData))
        {
            // Handle the subscription validation event.
            if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
            {
                var responseData = new SubscriptionValidationResponse
                {
                    ValidationResponse = subscriptionValidationEventData.ValidationCode
                };
                return Results.Ok(responseData);
            }

            if (eventData is AcsIncomingCallEventData acsIncomingCallEventData)
            {
                logger.LogInformation($"AcsIncomingCallEventData received: {JsonSerializer.Serialize(acsIncomingCallEventData, new JsonSerializerOptions { WriteIndented = true })}");

                var callbackUri = new Uri($"{configurationOptions.Value.CallBackUrl}?serverCallId={acsIncomingCallEventData.ServerCallId}");
                logger.LogInformation($"Callback Url: {callbackUri}");

                var websocketUri = new Uri($"{configurationOptions.Value.WebSocketUrl}?serverCallId={acsIncomingCallEventData.ServerCallId}");
                logger.LogInformation($"WebSocket Url: {websocketUri}");

                var mediaStreamingOptions = new MediaStreamingOptions(
                        websocketUri,
                        MediaStreamingContent.Audio,
                        MediaStreamingAudioChannel.Mixed,
                        startMediaStreaming: true
                        )
                {
                    EnableBidirectional = true,
                    AudioFormat = AudioFormat.Pcm24KMono,
                };

                var options = new AnswerCallOptions(acsIncomingCallEventData.IncomingCallContext, callbackUri)
                {
                    MediaStreamingOptions = mediaStreamingOptions,
                };

                AnswerCallResult answerCallResult = await client.AnswerCallAsync(options);
                logger.LogInformation($"Answered call for connection id: {answerCallResult.CallConnection.CallConnectionId}");
                return Results.Ok();
            }
        }
    }
    return Results.BadRequest();
});

app.UseWebSockets();

app.MapGet("/ws", async ([FromQuery] string serverCallId, HttpContext context, IOptions<VoiceRagOptions> configurationOptions, AzureOpenAIClient openAIClient, IAIToolRegistry toolRegistry, ILoggerFactory loggerFactory) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        try
        {
            var config = configurationOptions.Value;

            //var voiceClient = openAIClient.AsVoiceClient(config.AzureOpenAIDeploymentModelName, voiceClientLogger);
            var realtimeClient = openAIClient.GetRealtimeConversationClient(config.RealtimeOpenAIDeploymentModelName);

            IList<AITool> tools = [AIFunctionFactory.Create(GetRoomCapacity), AIFunctionFactory.Create(BookRoom)];

            RealtimeSessionOptions sessionOptions = new()
            {
                Instructions = config.RealtimeSystemPrompt,
                Voice = ConversationVoice.Coral,
                InputAudioFormat = ConversationAudioFormat.Pcm16,
                OutputAudioFormat = ConversationAudioFormat.Pcm16,
                Tools = tools.ToArray(),
                TurnDetectionOptions = ConversationTurnDetectionOptions.CreateServerVoiceActivityTurnDetectionOptions(0.5f, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)),
            };
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var conversation = new RealtimeAgentConversation(webSocket, realtimeClient, sessionOptions, loggerFactory);
            await conversation.StartAsync(context.RequestAborted);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception received {ex}");
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();


[Description("Returns the number of people that can fit in a room.")]
static int GetRoomCapacity(RoomType roomType)
{
return roomType switch
{
RoomType.ShuttleSimulator => throw new InvalidOperationException("No longer available"),
RoomType.NorthAtlantisLawn => 450,
RoomType.VehicleAssemblyBuilding => 12000,
_ => throw new NotSupportedException($"Unknown room type: {roomType}"),
};
}

[Description("Books a room and returns the confirmation number")]
static string BookRoom(
    RoomType roomType,
    string name,
    string phoneNumber)
{

if (string.IsNullOrWhiteSpace(name))
{
throw new ArgumentException("Name cannot be null or empty.", nameof(name));
}

if (string.IsNullOrWhiteSpace(phoneNumber))
{
throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
}
Console.WriteLine($"Booking room {roomType} for {name} with phone number {phoneNumber}.");
return roomType switch
{
RoomType.ShuttleSimulator => throw new InvalidOperationException("No longer available"),
RoomType.NorthAtlantisLawn => "1234",
RoomType.VehicleAssemblyBuilding => "9876",
_ => throw new NotSupportedException($"Unknown room type: {roomType}"),
};
}

enum RoomType
{
    ShuttleSimulator,
    NorthAtlantisLawn,
    VehicleAssemblyBuilding,
}
