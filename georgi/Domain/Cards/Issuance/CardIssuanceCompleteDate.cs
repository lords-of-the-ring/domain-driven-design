using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardIssuanceCompleteDate
{
    public static CardIssuanceCompleteDate From(IDateTime dateTime) => new();
}
