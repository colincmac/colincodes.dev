var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var openai = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
    : builder.AddConnectionString("openai");

var mcpServer = builder.AddProject<Projects.Showcase_EnterpriseMcp_Server>("mcpserver");

builder.AddProject<Projects.Showcase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(mcpServer)
    .WaitFor(mcpServer);

builder.AddProject<Projects.Showcase_AI_Voice_ProcessAgent>("showcase-ai-voice-processagent")
    .WithReference(openai)
    .WithEnvironment("OPENAI_EXPERIMENTAL_ENABLE_OPEN_TELEMETRY", "true");

builder.Build().Run();
