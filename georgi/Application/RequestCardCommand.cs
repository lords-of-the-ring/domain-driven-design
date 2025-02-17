using Domain.Accounts;
using Domain.Cards;

namespace Application;

public sealed record RequestCardCommand
{
    public required AccountId AccountId { get; init; }

    public required CardType CardType { get; init; }

    public required CardIssuerId CardIssuerId { get; init; }
}
