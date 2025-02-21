using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuers;

namespace Application.Features.Cards.Issuance.RequestCardIssuance;

public sealed record RequestCardIssuanceCommand
{
    public required AccountId AccountId { get; init; }

    public required CardType CardType { get; init; }

    public required CardIssuerId CardIssuerId { get; init; }
}
