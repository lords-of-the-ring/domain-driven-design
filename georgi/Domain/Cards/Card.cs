using Domain.Abstractions;
using Domain.Accounts;

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

    public void RequestStatus(CardStatus newStatus)
    {
        if (CurrentStatus == CardStatus.Terminated)
        {
            return;
        }

        if (RequestedStatus == CardStatus.Terminated)
        {
            return;
        }

        if (newStatus == CardStatus.Terminated)
        {
            RequestedStatus = CardStatus.Terminated;
            return;
        }

        if (RequestedStatus is null)
        {
            if (CurrentStatus == CardStatus.Issued && newStatus != CardStatus.Active)
            {
                throw new Exception();
            }

            if (CurrentStatus == CardStatus.Active && newStatus != CardStatus.Blocked)
            {
                throw new Exception();
            }

            if (CurrentStatus == CardStatus.Blocked && newStatus != CardStatus.Active)
            {
                throw new Exception();
            }

            RequestedStatus = newStatus;
            return;
        }

        throw new Exception();
    }

    public void CompleteStatusChange(CardStatus requestedStatus)
    {
        throw new NotImplementedException();
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
            RequestedStatus = CardStatus.Issued,
        };

        return card;
    }
}
