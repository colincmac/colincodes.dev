using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Showcase.McpServer.Models;
using Showcase.McpServer.Services;
using Showcase.McpServer.Tools;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.Configure<McpServerOptions>(builder.Configuration.GetRequiredSection(McpServerOptions.SectionName));

// Add services to the container.
builder.Services.AddProblemDetails();
// Authentication & Authorization
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
//builder.Services.AddAuthorization();
var serverUrl = "http://localhost:7071/";
var tenantId = "a2213e1c-e51e-4304-9a0d-effe57f31655";
var instance = "https://login.microsoftonline.com/";


builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.Authority = $"{instance}{tenantId}/v2.0";
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidAudience = "167b4284-3f92-4436-92ed-38b38f83ae08",
//        ValidIssuer = $"{instance}{tenantId}/v2.0",
//        NameClaimType = "name",
//        RoleClaimType = "roles"
//    };

//    options.MetadataAddress = $"{instance}{tenantId}/v2.0/.well-known/openid-configuration";

//    options.Events = new JwtBearerEvents
//    {
//        OnTokenValidated = context =>
//        {
//            var name = context.Principal?.Identity?.Name ?? "unknown";
//            var email = context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
//            Console.WriteLine($"Token validated for: {name} ({email})");
//            return Task.CompletedTask;
//        },
//        OnAuthenticationFailed = context =>
//        {
//            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
//            return Task.CompletedTask;
//        },
//        OnChallenge = context =>
//        {
//            Console.WriteLine($"Challenging client to authenticate with Entra ID");
//            return Task.CompletedTask;
//        }
//    };
//})
//.AddMcp(options =>
//{
//    //options.ProtectedResourceMetadataProvider = context =>
//    //{
//    //    var metadata = new ProtectedResourceMetadata
//    //    {
//    //        Resource = new Uri("http://localhost"),
//    //        BearerMethodsSupported = { "header" },
//    //        ResourceDocumentation = new Uri("https://docs.example.com/api/weather"),
//    //        AuthorizationServers = { new Uri($"{instance}{tenantId}/v2.0") }
//    //    };

//    //    metadata.ScopesSupported.AddRange([
//    //        "api://167b4284-3f92-4436-92ed-38b38f83ae08/weather.read"
//    //    ]);

//    //    return metadata;
//    //};
//});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMcpServer()
    .WithTools<WeatherTools>()
    .WithHttpTransport();

builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.weather.gov");
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
});

// Dapr client
builder.Services.AddDaprClient();

// MCP infrastructure services
builder.Services.AddSingleton<IToolSchemaRegistry, ToolSchemaRegistry>();
builder.Services.AddSingleton<IJsonSchemaValidator, JsonSchemaValidator>();
builder.Services.AddSingleton<IToolMonitor, ToolMonitor>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapMcp().RequireAuthorization();


// Load allowed tools list from configuration
var allowedTools = builder.Configuration.GetSection("AllowedTools").Get<string[]>() ?? Array.Empty<string>();
// Endpoint for invoking MCP tools
app.MapPost("/api/mcp/invoke", [Authorize] async (ToolInvokeRequest request, DaprClient dapr, IToolSchemaRegistry schemaRegistry, IJsonSchemaValidator validator, IToolMonitor monitor) =>
{
    // 1. Allowlist check
    if (!allowedTools.Contains(request.ToolName))
        return Results.BadRequest("Tool not allowed.");
    // 2. Validate input schema
    var inputSchema = schemaRegistry.GetInputSchema(request.ToolName);
    if (!validator.IsValid(inputSchema, request.Input))
        return Results.BadRequest("Invalid input for tool.");
    // 3. Invoke tool via Dapr
    var toolResult = await dapr.InvokeMethodAsync<object, ToolOutput>(HttpMethod.Post, request.ToolName, "run", request.Input);
    // 4. Validate output schema
    var outputSchema = schemaRegistry.GetOutputSchema(request.ToolName);
    if (!validator.IsValid(outputSchema, toolResult))
        return Results.StatusCode(500);
    // 5. Sanitize and monitor
    monitor.Sanitize(toolResult);
    monitor.Log(request.UserId, request.ToolName, request.Input, toolResult);
    return Results.Ok(toolResult);
})
   .WithName("InvokeTool");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
