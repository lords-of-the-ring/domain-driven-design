using Domain.Abstractions;
using Domain.Accounts;
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

    public void RequestStatusChange(CardStatus newStatus)
    {
        if (CurrentStatus is CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.CurrentStatusIsAlreadyTerminated);
        }

        if (RequestedStatus is CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.RequestedStatusIsAlreadyTerminated);
        }

        if (newStatus is CardStatus.Terminated && CurrentStatus is CardStatus.Requested)
        {
            throw new CardDomainException(CardId, Errors.NewStatusCannotBeTerminatedWhenCurrentStatusIsRequested);
        }

        if (newStatus is CardStatus.Terminated)
        {
            RequestedStatus = CardStatus.Terminated;
            return;
        }

        if (RequestedStatus is not null)
        {
            throw new CardDomainException(CardId, Errors.RequestedStatusMustBeNullWhenNewStatusIsNotTerminated);
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

        if (CurrentStatus is CardStatus.Requested && expectedStatus is CardStatus.Terminated)
        {
            throw new CardDomainException(CardId, Errors.ExpectedStatusCannotBeTerminatedWhenCurrentStatusIsRequested);
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

    public static class Errors
    {
        // Request status change errors
        public const string CurrentStatusIsAlreadyTerminated = "Current status is already Terminated";
        public const string RequestedStatusIsAlreadyTerminated = "Requested status is already Terminated";

        public const string NewStatusCannotBeTerminatedWhenCurrentStatusIsRequested =
            "New status cannot be Terminated when current status is Requested";

        public const string RequestedStatusMustBeNullWhenNewStatusIsNotTerminated =
            "Requested status must be null when new status is not Terminated";

        // Complete status change errors
        public const string ExpectedStatusMustBeTheSameAsRequestedStatus =
            "Expected status must be the same as requested status";

        public const string ExpectedStatusMustBeTerminated =
            "Expected status must be Terminated when requested status is null";

        public const string ExpectedStatusCannotBeTerminatedWhenCurrentStatusIsRequested =
            "Expected status cannot be Terminated when current status is Requested";

        // Validate status compatibility errors
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
    }
}
