namespace Domain.Cards.Issuance;

public sealed record CardIssuanceRequestedDomainEvent
{
    public required Card Card { get; init; }

    public required CardIssuance CardIssuance { get; init; }
}
