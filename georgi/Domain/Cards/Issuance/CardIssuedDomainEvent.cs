using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardIssuedDomainEvent : DomainEvent
{
    public required Card Card { get; init; }
}
