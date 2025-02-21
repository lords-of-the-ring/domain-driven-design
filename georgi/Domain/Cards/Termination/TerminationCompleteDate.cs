using Domain.Abstractions;

namespace Domain.Cards.Termination;

public sealed record TerminationCompleteDate
{
    private TerminationCompleteDate() { }

    public required DateTimeOffset Value { get; init; }

    public static TerminationCompleteDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
