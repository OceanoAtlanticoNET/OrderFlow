using OrderFlow.Shared.Events;

namespace OrderFlow.Identity.Services.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(string channel, T @event, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}
