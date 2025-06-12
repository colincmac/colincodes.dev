using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Showcase.Authentication.Core;

/// <summary>Source-generated JSON type information.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(IDictionary<string, object?>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Uri[]))]
[JsonSerializable(typeof(ProtectedResourceMetadata))]
internal sealed partial class JsonContext : JsonSerializerContext;

