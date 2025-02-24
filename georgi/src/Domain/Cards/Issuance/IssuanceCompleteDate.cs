using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record IssuanceCompleteDate
{
    public static IssuanceCompleteDate From(IDateTime dateTime) => new();
}
