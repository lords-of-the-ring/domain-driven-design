namespace Domain.Cards.Termination;

public sealed record CardTerminationRequestedDomainEvent
{
    public required CardTermination CardTermination { get; init; }
}
