using Microsoft.Extensions.Logging;
using ModelContextProtocol.Authentication;
using ModelContextProtocol.Client;

namespace ModelContextProtocol.Protocol.Transport;

/// <summary>
/// Provides an <see cref="IClientTransport"/> over HTTP using the Server-Sent Events (SSE) protocol.
/// </summary>
/// <remarks>
/// This transport connects to an MCP server over HTTP using SSE,
/// allowing for real-time server-to-client communication with a standard HTTP request.
/// Unlike the <see cref="StdioClientTransport"/>, this transport connects to an existing server
/// rather than launching a new process.
/// </remarks>
public sealed class SecureSseClientTransport : IClientTransport, IAsyncDisposable
{
    private readonly SseClientTransportOptions _options;
    private readonly SseClientTransport _innerTransport;
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SseClientTransport"/> class with authentication support.
    /// </summary>
    /// <param name="transportOptions">Configuration options for the transport.</param>
    /// <param name="credentialProvider">The authorization provider to use for authentication.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers used for diagnostic output during transport operations.</param>
    /// <param name="baseMessageHandler">Optional. The base message handler to use under the authorization handler. 
    /// If null, a new <see cref="HttpClientHandler"/> will be used. This allows for custom HTTP client pipelines (e.g., from HttpClientFactory) 
    /// to be used in conjunction with the token-based authentication provided by <paramref name="credentialProvider"/>.</param>
    public SecureSseClientTransport(SseClientTransportOptions transportOptions, IMcpCredentialProvider credentialProvider, ILoggerFactory? loggerFactory = null, HttpMessageHandler? baseMessageHandler = null)
    {
        ArgumentNullException.ThrowIfNull(transportOptions);
        ArgumentNullException.ThrowIfNull(credentialProvider);

        _options = transportOptions;
        _loggerFactory = loggerFactory;
        Name = transportOptions.Name ?? transportOptions.Endpoint.ToString();
        var authHandler = new AuthorizationDelegatingHandler(credentialProvider)
        {
            InnerHandler = baseMessageHandler ?? new HttpClientHandler()
        };
        
        _httpClient = new HttpClient(authHandler);
        _innerTransport = new SseClientTransport(_options, _httpClient, _loggerFactory, false);
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public async Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _innerTransport.ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return default;
    }
}