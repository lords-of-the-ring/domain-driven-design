namespace Domain.Abstractions;

public abstract class DomainEntity
{
    private readonly HashSet<DomainEvent> _domainEvents = [];

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.ToList();

    protected void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    internal void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
}
