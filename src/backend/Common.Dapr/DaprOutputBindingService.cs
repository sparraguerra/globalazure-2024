using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace Common.Dapr;

public class DaprPayloadMessageSignalR
{
    public string Target { get; set; } = string.Empty;
    public Argument[]? Arguments { get; set; }
}

public class Argument
{
    public string Sender { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public interface IDaprOutputBindingService
{
    Task PublishMessageAsync<T>(T message, string bindingName, CancellationToken cancellationToken);
    Task PublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken);
    Task PublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken);
    Task TryPublishMessageAsync<T>(T message, string bindingName, CancellationToken cancellationToken);
    Task TryPublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken);
    Task TryPublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken);
    
}

public class DaprOutputBindingService : IDaprOutputBindingService
{
    private readonly DaprClient client;
    private readonly ILogger<DaprOutputBindingService> logger;

    public DaprOutputBindingService(DaprClient client, ILogger<DaprOutputBindingService> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task PublishMessageAsync<T>(T message, string bindingName, CancellationToken cancellationToken) =>
        await PublishMessageAsync(message, default, bindingName, cancellationToken);

    public async Task PublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken) =>
       await PublishMessageAsync(message, "create", metadata, bindingName, cancellationToken);

    public async Task PublishMessageAsync<T>(T message, string operation,  IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing message {Message} to {BindingName}", message, bindingName);

        await client.InvokeBindingAsync(bindingName, operation, message, metadata);
    }

    public async Task TryPublishMessageAsync<T>(T message, string bindingName, CancellationToken cancellationToken) =>
        await TryPublishMessageAsync(message, default, bindingName, cancellationToken);

    public async Task TryPublishMessageAsync<T>(T message, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken) =>
       await TryPublishMessageAsync(message, "create", metadata, bindingName, cancellationToken);

    public async Task TryPublishMessageAsync<T>(T message, string operation, IReadOnlyDictionary<string, string>? metadata, string bindingName, CancellationToken cancellationToken)
    {
        try
        {
            await PublishMessageAsync(message, operation, metadata, bindingName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError("Error publishing message {Message} to {BindingName}: {MessageException}", message, bindingName, ex.Message);
        }
    }

}
