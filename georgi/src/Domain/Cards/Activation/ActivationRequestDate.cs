using Domain.Abstractions;

namespace Domain.Cards.Activation;

public sealed record ActivationRequestDate : ValueObject<DateTimeOffset>
{
    private ActivationRequestDate() { }

    public static ActivationRequestDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
