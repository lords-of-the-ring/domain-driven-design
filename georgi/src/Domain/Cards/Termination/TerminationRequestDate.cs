using Domain.Abstractions;

namespace Domain.Cards.Termination;

public sealed record TerminationRequestDate
{
    private TerminationRequestDate() { }

    public required DateTimeOffset Value { get; init; }

    public static TerminationRequestDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
