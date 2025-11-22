namespace OrderFlow.Shared.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
}
