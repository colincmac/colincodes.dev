var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var mcpServer = builder.AddProject<Projects.Showcase_McpServer>("mcpserver");

builder.AddProject<Projects.Showcase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(mcpServer)
    .WaitFor(mcpServer);

builder.Build().Run();
