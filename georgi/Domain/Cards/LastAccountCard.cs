using Domain.Accounts;
using Domain.Cards.Issuance;

namespace Domain.Cards;

public sealed record LastAccountCard
{
    private LastAccountCard() { }

    public required LastAccountCardValue? Value { get; init; }
}

public sealed record LastAccountCardValue
{
    public required Card Card { get; init; }

    public required CardIssuance CardIssuance { get; init; }
}
