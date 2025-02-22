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

        ValidateStatusCompatibility(newStatus);
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
            ValidateStatusCompatibility(expectedStatus);
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
            throw new CardDomainException(CardId, Errors.ExpectedStatusMustBeTheSameAsRequestedStatus);
        }

        ValidateStatusCompatibility(expectedStatus);
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

    internal void ValidateStatusCompatibility(CardStatus nextStatus)
    {
        if (CurrentStatus is CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.CurrentStatusCannotBeTerminated);
        }

        if (nextStatus is CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.NextStatusCannotBeTerminated);
        }

        if (CurrentStatus is CardStatus.Requested && nextStatus is not CardStatus.Issued)
        {
            throw new CardDomainException(CardId, Errors.NextStatusMustBeIssuedWhenCurrentStatusIsRequested);
        }

        if (CurrentStatus is CardStatus.Issued && nextStatus is not CardStatus.Active)
        {
            throw new CardDomainException(CardId, Errors.NextStatusMustBeActiveWhenCurrentStatusIsIssued);
        }

        if (CurrentStatus is CardStatus.Active && nextStatus is not CardStatus.Blocked)
        {
            throw new CardDomainException(CardId, Errors.NextStatusMustBeBlockedWhenCurrentStatusIsActive);
        }

        if (CurrentStatus is CardStatus.Blocked && nextStatus is not CardStatus.Active)
        {
            throw new CardDomainException(CardId, Errors.NextStatusMustBeActiveWhenCurrentStatusIsBlocked);
        }
    }

    public static class Errors
    {
        public const string CurrentStatusCannotBeTerminated = "Current status cannot be Terminated";

        public const string NextStatusCannotBeTerminated = "Next status cannot be Terminated";

        public const string NextStatusMustBeActiveWhenCurrentStatusIsIssued =
            "Next status must be Active when current status is Issued";

        public const string NextStatusMustBeIssuedWhenCurrentStatusIsRequested =
            "Next status must be Issued when current status is Requested";

        public const string NextStatusMustBeBlockedWhenCurrentStatusIsActive =
            "Next status must be Blocked when current status is Active";

        public const string NextStatusMustBeActiveWhenCurrentStatusIsBlocked =
            "Next status must be Active when current status is Blocked";

        public const string ExpectedStatusMustBeTheSameAsRequestedStatus =
            "Expected status must be the same as requested status";

        public const string ExpectedStatusMustBeTerminated =
            "Expected status must be Terminated when requested status is null";
    }
}
