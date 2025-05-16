using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Dapr.Client;
using Showcase.McpServer.Models;
using Showcase.McpServer.Services;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();
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

// TODO: Remove sample endpoint when implementing MCP endpoints
string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

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
