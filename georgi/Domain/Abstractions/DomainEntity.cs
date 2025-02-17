namespace Domain.Abstractions;

public abstract class DomainEntity
{
    protected void RaiseDomainEvent(object domainEvent) { }
}
