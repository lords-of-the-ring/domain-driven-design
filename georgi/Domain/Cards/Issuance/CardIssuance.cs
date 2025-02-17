using Domain.Abstractions;
using Domain.Accounts;
using Domain.Credits;
using Domain.Users;

using ErrorOr;

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

    public void Complete(IDateTime dateTime)
    {
        if (CompleteDate is not null)
        {
            return;
        }

        Card.CompleteStatusChange(CardStatus.Issued);
        CompleteDate = CardIssuanceCompleteDate.From(dateTime);
    }

    public static CardIssuance Request(
        UserId userId,
        Credit credit,
        AccountId accountId,
        LastAccountCard lastCard,
        AccountBlockInfo blockInfo,
        CardType cardType,
        CardIssuerId cardIssuerId,
        IDateTime dateTime)
    {
        if (lastCard.Value is null)
        {
            var result = CheckIfRequestingInitialCardIsAllowed(credit);
            result.ThrowIfError();
        }
        else
        {
            var result = CheckIfRequestingRenewedCardIsAllowed(blockInfo, lastCard, dateTime);
            result.ThrowIfError();
        }

        var card = Card.Create(accountId, cardType, cardIssuerId);

        var cardIssuance = new CardIssuance
        {
            CardId = card.CardId, Card = card, UserId = userId, RequestDate = CardIssuanceRequestDate.From(dateTime)
        };

        cardIssuance.RaiseDomainEvent(
            new CardIssuanceRequestedDomainEvent { Card = card, CardIssuance = cardIssuance });

        return cardIssuance;
    }

    public static ErrorOr<Success> CheckIfRequestingInitialCardIsAllowed(Credit credit)
    {
        if (credit.Status != CreditStatus.Active)
        {
            return Error.Validation(Errors.CreditStatusMustBeActive);
        }

        if (credit.Type != CreditType.Regular)
        {
            return Error.Validation(Errors.CreditTypeMustBeRegular);
        }

        return new Success();
    }

    public static ErrorOr<Success> CheckIfRequestingRenewedCardIsAllowed(AccountBlockInfo accountBlockInfo,
        LastAccountCard lastCard,
        IDateTime dateTime)
    {
        if (accountBlockInfo.HasPendingBlocks)
        {
            return Error.Validation(Errors.PendingAccountBlocksPresent);
        }

        ArgumentNullException.ThrowIfNull(lastCard.Value, nameof(lastCard.Value));

        if (lastCard.Value.CardIssuance.CardExpiryDate is null)
        {
            return Error.Validation(Errors.CardExpiryDateIsMissing);
        }

        if (lastCard.Value.Card.HasExactStatus(CardStatus.Active))
        {
            if (!lastCard.Value.CardIssuance.CardExpiryDate.IsInPreExpiryPeriod(dateTime))
            {
                return Error.Validation(Errors.CardNotInPreExpiryPeriod);
            }

            return new Success();
        }

        if (lastCard.Value.Card.HasExactStatus(CardStatus.Terminated))
        {
            if (!lastCard.Value.CardIssuance.CardExpiryDate.IsInAfterExpiryPeriod(dateTime))
            {
                return Error.Validation(Errors.CardNotInAfterExpiryPeriod);
            }

            return new Success();
        }

        return Error.Validation(Errors.CardExactStatusMustBeActiveOrTerminated);
    }

    private static class Errors
    {
        public const string CreditStatusMustBeActive = "Credit status must be Active";
        public const string CreditTypeMustBeRegular = "Credit type must be Regular";
        public const string PendingAccountBlocksPresent = "Pending account blocks are present";
        public const string CardExpiryDateIsMissing = "Card expiry date is missing";
        public const string CardNotInPreExpiryPeriod = "Card is not in pre expiry period";
        public const string CardNotInAfterExpiryPeriod = "Card is not in after expiry period";
        public const string CardExactStatusMustBeActiveOrTerminated = "Card exact status must be Active or Terminated";
    }
}
