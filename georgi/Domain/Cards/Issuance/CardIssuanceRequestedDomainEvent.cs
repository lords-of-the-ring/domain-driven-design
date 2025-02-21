namespace Domain.Cards.Issuance;

public sealed record CardIssuanceRequestedDomainEvent
{
    public required CardIssuance CardIssuance { get; init; }
}
