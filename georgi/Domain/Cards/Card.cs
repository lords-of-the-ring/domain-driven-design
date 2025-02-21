using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards.Issuance;
using Domain.Cards.Issuers;

namespace Domain.Cards;

public sealed class Card : DomainEntity
{
    private Card() { }

    public required CardId CardId { get; init; }

    public required AccountId AccountId { get; init; }

    public required CardType Type { get; init; }

    public required CardIssuerId IssuerId { get; init; }

    public CardStatus CurrentStatus { get; private set; }

    public CardStatus? RequestedStatus { get; private set; }

    public bool HasExactStatus(CardStatus status) => CurrentStatus == status && RequestedStatus is null;

    public void RequestStatusChange(CardStatus newStatus)
    {
        if (CurrentStatus is CardStatus.Terminated)
        {
            return;
        }

        if (RequestedStatus is CardStatus.Terminated)
        {
            return;
        }

        if (newStatus is CardStatus.Terminated)
        {
            RequestedStatus = CardStatus.Terminated;
            return;
        }

        if (RequestedStatus is not null)
        {
            throw new CardDomainException(CardId, $"Requested status must be null, but was {RequestedStatus}.");
        }

        CheckIfCurrentStatusIsCompatible(newStatus);
        RequestedStatus = newStatus;
    }

    public void CompleteStatusChange(CardStatus expectedStatus)
    {
        if (CurrentStatus is CardStatus.Terminated)
        {
            return;
        }

        if (RequestedStatus is null && expectedStatus is not CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.ExpectedStatusMustBeTerminated);
        }

        if (RequestedStatus is null)
        {
            CurrentStatus = CardStatus.Terminated;
            return;
        }

        if (RequestedStatus is CardStatus.Terminated && expectedStatus is CardStatus.Terminated)
        {
            CurrentStatus = CardStatus.Terminated;
            RequestedStatus = null;
            return;
        }

        if (RequestedStatus is CardStatus.Terminated)
        {
            CheckIfCurrentStatusIsCompatible(expectedStatus);
            CurrentStatus = expectedStatus;
            return;
        }

        if (expectedStatus is CardStatus.Terminated)
        {
            CurrentStatus = CardStatus.Terminated;
            RequestedStatus = null;
            return;
        }

        if (RequestedStatus != expectedStatus)
        {
            throw new CardDomainException(CardId,
                $"Expected status must be the same as requested status, but was {expectedStatus}.");
        }

        CheckIfCurrentStatusIsCompatible(expectedStatus);
        CurrentStatus = expectedStatus;
        RequestedStatus = null;
    }

    public static Card Create(AccountId accountId, CardType cardType, CardIssuerId cardIssuerId)
    {
        var card = new Card
        {
            CardId = CardId.New(),
            AccountId = accountId,
            Type = cardType,
            IssuerId = cardIssuerId,
            CurrentStatus = CardStatus.Requested,
        };

        return card;
    }

    private void CheckIfCurrentStatusIsCompatible(CardStatus newStatus)
    {
        if (CurrentStatus == CardStatus.Requested && newStatus is not CardStatus.Issued)
        {
            throw new CardDomainException(CardId,
                $"Requested status must be Issued when current status is Requested, but was {newStatus}.");
        }

        if (CurrentStatus == CardStatus.Issued && newStatus is not CardStatus.Active)
        {
            throw new CardDomainException(CardId,
                $"Requested status must be Active when current status is Issued, but was {newStatus}.");
        }

        if (CurrentStatus == CardStatus.Active && newStatus is not CardStatus.Blocked)
        {
            throw new CardDomainException(CardId,
                $"Requested status must be Blocked when current status is Active, but was {newStatus}.");
        }

        if (CurrentStatus == CardStatus.Blocked && newStatus is not CardStatus.Active)
        {
            throw new CardDomainException(CardId,
                $"Requested status must be Active when current status is Blocked, but was {newStatus}.");
        }
    }

    public static class Errors
    {
        public const string ExpectedStatusMustBeTerminated =
            "Expected status must be Terminated when requested status is null";
    }
}
