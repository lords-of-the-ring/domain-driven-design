using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record IssuanceRequestDate
{
    private IssuanceRequestDate() { }

    public required DateTimeOffset Value { get; init; }

    public static IssuanceRequestDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
