using Domain.Abstractions;

namespace Domain.Cards.Activation;

public sealed record ActivationCompleteDate : ValueObject<DateTimeOffset>
{
    private ActivationCompleteDate() { }

    public static ActivationCompleteDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
