using Domain.Abstractions;

namespace Domain.Cards.Termination;

public sealed record CardTerminationRequestedDomainEvent : DomainEvent
{
    public required CardTermination CardTermination { get; init; }
}
