using Domain.Cards;
using Domain.Cards.Issuance;

namespace Application.Features.Cards.Issuance.CompleteCardIssuance;

public sealed record CompleteCardIssuanceCommand
{
    public required CardId CardId { get; init; }

    public required CardPan CardPan { get; init; }

    public required CardExpiryDate CardExpiryDate { get; init; }
}
