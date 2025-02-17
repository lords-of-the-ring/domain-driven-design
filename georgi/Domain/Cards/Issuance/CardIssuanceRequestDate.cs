using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardIssuanceRequestDate
{
    public static CardIssuanceRequestDate From(IDateTime dateTime) => new();
}
