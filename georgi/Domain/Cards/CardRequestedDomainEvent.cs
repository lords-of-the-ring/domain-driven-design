namespace Domain.Cards;

public sealed record CardRequestedDomainEvent
{
    public required Card Card { get; init; }
}
