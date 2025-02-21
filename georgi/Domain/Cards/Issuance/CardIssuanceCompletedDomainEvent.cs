using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardIssuanceCompletedDomainEvent : DomainEvent
{
    public required Card Card { get; init; }
}
