using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record IssuanceRequestDate
{
    public static IssuanceRequestDate From(IDateTime dateTime) => new();
}
