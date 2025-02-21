namespace Domain.Cards.Issuance;

public sealed record CardIssuanceCompletedDomainEvent
{
    public required Card Card { get; init; }
}
