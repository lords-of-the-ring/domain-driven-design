using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record IssuanceCompleteDate
{
    private IssuanceCompleteDate() { }

    public required DateTimeOffset Value { get; init; }

    public static IssuanceCompleteDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
