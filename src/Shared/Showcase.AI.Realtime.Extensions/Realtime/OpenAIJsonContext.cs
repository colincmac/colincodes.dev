using System.Text.Json;
using System.Text.Json.Serialization;
using Showcase.AI.Realtime.Extensions.Realtime;

namespace Showcase.Shared.AIExtensions.Realtime;

/// <summary>Source-generated JSON type information.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(OpenAIRealtimeExtensions.ConversationFunctionToolParametersSchema))]
internal sealed partial class OpenAIJsonContext : JsonSerializerContext;