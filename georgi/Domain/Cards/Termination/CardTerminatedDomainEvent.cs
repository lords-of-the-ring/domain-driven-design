using Domain.Abstractions;

namespace Domain.Cards.Termination;

public sealed record CardTerminatedDomainEvent : DomainEvent
{
    public required CardTermination CardTermination { get; init; }
}
