using Domain.Abstractions;

namespace Domain.Cards.Activation;

public sealed record CardActivationRequestedDomainEvent : DomainEvent
{
    public required CardActivation CardActivation { get; init; }
}
