namespace Domain.Cards.Termination;

public sealed record CardTerminatedDomainEvent
{
    public required CardTermination CardTermination { get; init; }
}
