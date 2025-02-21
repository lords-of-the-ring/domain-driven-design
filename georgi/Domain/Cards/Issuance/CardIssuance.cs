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

    public required IssuanceRequestDate RequestDate { get; init; }

    public IssuanceCompleteDate? CompleteDate { get; private set; }

    public CardExpiryDate? CardExpiryDate { get; private set; }

    public CardPan? CardPan { get; private set; }

    public void Complete(IDateTime dateTime, CardExpiryDate expiryDate, CardPan cardPan)
    {
        if (CompleteDate is not null)
        {
            return;
        }

        CompleteDate = IssuanceCompleteDate.From(dateTime);
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
        AccountBlockInfo accountBlockInfo,
        CardType cardType,
        CardIssuerId cardIssuerId,
        IDateTime dateTime,
        ICardIssuanceRepository cardIssuanceRepository)
    {
        if (lastCardIssuance.Value is null)
        {
            CheckIfRequestingInitialCardIssuanceIsAllowed(credit);
        }
        else
        {
            CheckIfRequestingCardIssuanceRenewalIsAllowed(accountBlockInfo, lastCardIssuance);
        }

        var card = Card.Create(accountId, cardType, cardIssuerId);

        var cardIssuance = new CardIssuance
        {
            CardId = card.CardId, Card = card, UserId = userId, RequestDate = IssuanceRequestDate.From(dateTime)
        };

        cardIssuanceRepository.AddCardIssuance(cardIssuance);
        card.RequestStatusChange(CardStatus.Issued);
        cardIssuance.RaiseDomainEvent(new CardIssuanceRequestedDomainEvent { CardIssuance = cardIssuance });

        return card.CardId;
    }

    private static void CheckIfRequestingInitialCardIssuanceIsAllowed(Credit credit)
    {
        if (credit.Status != CreditStatus.Active)
        {
            throw new CreditDomainException(credit.CreditId, Errors.CreditStatusMustBeActive);
        }

        if (credit.Type != CreditType.Regular)
        {
            throw new CreditDomainException(credit.CreditId, Errors.CreditTypeMustBeRegular);
        }
    }

    private static void CheckIfRequestingCardIssuanceRenewalIsAllowed(AccountBlockInfo blockInfo,
        LastAccountCardIssuance lastCardIssuance)
    {
        ArgumentNullException.ThrowIfNull(lastCardIssuance.Value);

        if (blockInfo.HasPendingBlocks)
        {
            throw new CardDomainException(lastCardIssuance.Value.CardId, Errors.PendingAccountBlocksArePresent);
        }
    }

    public static class Errors
    {
        public const string CreditStatusMustBeActive = "Credit status must be Active";
        public const string CreditTypeMustBeRegular = "Credit type must be Regular";
        public const string PendingAccountBlocksArePresent = "Pending account blocks are present";
    }
}
