using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardIssuanceRequestedDomainEvent : DomainEvent
{
    public required CardIssuance CardIssuance { get; init; }
}
