namespace Application.Abstractions;

public interface IEventDispatcher
{
    Task DispatchIntegrationEvent<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken);
}
