using System.Text.Json;
using OrderFlow.Shared.Events;
using StackExchange.Redis;

namespace OrderFlow.Identity.Services.Events;

public class RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger) : IEventPublisher
{
    public async Task PublishAsync<T>(string channel, T @event, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var subscriber = redis.GetSubscriber();
        var message = JsonSerializer.Serialize(@event);

        await subscriber.PublishAsync(RedisChannel.Literal(channel), message);

        logger.LogInformation(
            "Published event {EventType} to channel {Channel} with EventId {EventId}",
            typeof(T).Name,
            channel,
            @event.EventId);
    }
}
