using MassTransit;

namespace OrderFlow.Shared.Events;

[ExcludeFromTopology]
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
}
