using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards.Issuers;
using Domain.Credits;
using Domain.Users;

namespace Domain.Cards.Issuance;

public sealed class CardIssuance : DomainEntity
{
    private CardIssuance() { }

    public required CardId CardId { get; init; }

    public required Card Card { get; init; }

    public required UserId UserId { get; init; }

    public required CardIssuanceRequestDate RequestDate { get; init; }

    public CardIssuanceCompleteDate? CompleteDate { get; private set; }

    public CardExpiryDate? CardExpiryDate { get; private set; }

    public CardPan? CardPan { get; private set; }

    public void Complete(IDateTime dateTime, CardExpiryDate expiryDate, CardPan cardPan)
    {
        if (CompleteDate is not null)
        {
            return;
        }

        CompleteDate = CardIssuanceCompleteDate.From(dateTime);
        CardExpiryDate = expiryDate;
        CardPan = cardPan;
        Card.CompleteStatusChange(CardStatus.Issued);
        RaiseDomainEvent(new CardIssuanceCompletedDomainEvent { Card = Card });
    }

    public static CardId Request(
        UserId userId,
        Credit credit,
        AccountId accountId,
        LastAccountCardIssuance lastCardIssuance,
        AccountBlockInfo blockInfo,
        CardType cardType,
        CardIssuerId cardIssuerId,
        IDateTime dateTime,
        ICardIssuanceRepository cardIssuanceRepository)
    {
        if (lastCardIssuance.Value is null)
        {
            CheckIfRequestingInitialCardIsAllowed(credit);
        }
        else
        {
            CheckIfRequestingRenewedCardIsAllowed(blockInfo, lastCardIssuance);
        }

        var card = Card.Create(accountId, cardType, cardIssuerId);

        var cardIssuance = new CardIssuance
        {
            CardId = card.CardId, Card = card, UserId = userId, RequestDate = CardIssuanceRequestDate.From(dateTime)
        };

        cardIssuanceRepository.AddCardIssuance(cardIssuance);
        card.RequestStatus(CardStatus.Requested);
        cardIssuance.RaiseDomainEvent(new CardIssuanceRequestedDomainEvent { CardIssuance = cardIssuance });

        return card.CardId;
    }

    private static void CheckIfRequestingInitialCardIsAllowed(Credit credit)
    {
        if (credit.Status != CreditStatus.Active)
        {
            throw new CreditDomainException(credit.CreditId, "Credit status must be Active.");
        }

        if (credit.Type != CreditType.Regular)
        {
            throw new CreditDomainException(credit.CreditId, "Credit type must be Regular.");
        }
    }

    private static void CheckIfRequestingRenewedCardIsAllowed(AccountBlockInfo blockInfo,
        LastAccountCardIssuance lastCardIssuance)
    {
        ArgumentNullException.ThrowIfNull(lastCardIssuance.Value);

        if (blockInfo.HasPendingBlocks)
        {
            throw new CardDomainException(lastCardIssuance.Value.CardId, "Pending account blocks are present.");
        }
    }
}
