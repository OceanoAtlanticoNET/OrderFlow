using System.Text.Json;
using OrderFlow.Notifications.Services;
using OrderFlow.Shared.Events;
using OrderFlow.Shared.Redis;
using StackExchange.Redis;

namespace OrderFlow.Notifications;

public class Worker(
    IConnectionMultiplexer redis,
    IServiceScopeFactory scopeFactory,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redis.GetSubscriber();

        logger.LogInformation("Notifications Worker started. Subscribing to Redis channels...");

        // Subscribe to user registered events
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisChannels.UserRegistered),
            async (channel, message) =>
            {
                await HandleUserRegisteredAsync(message!, stoppingToken);
            });

        logger.LogInformation("Subscribed to channel: {Channel}", RedisChannels.UserRegistered);

        // Keep the worker running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUserRegisteredAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
            if (@event is null)
            {
                logger.LogWarning("Failed to deserialize UserRegisteredEvent");
                return;
            }

            logger.LogInformation(
                "Received UserRegisteredEvent: EventId={EventId}, UserId={UserId}, Email={Email}",
                @event.EventId, @event.UserId, @event.Email);

            // Create a scope to resolve scoped services
            using var scope = scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendWelcomeEmailAsync(@event.Email, @event.FirstName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserRegisteredEvent: {Message}", message);
        }
    }
}
