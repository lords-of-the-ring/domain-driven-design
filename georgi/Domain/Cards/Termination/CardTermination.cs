using Domain.Abstractions;
using Domain.Users;

namespace Domain.Cards.Termination;

public sealed class CardTermination : DomainEntity
{
    private CardTermination() { }

    public required CardId CardId { get; init; }

    public required Card Card { get; init; }

    public required TerminationReason Reason { get; init; }

    public required UserId UserId { get; init; }

    public required TerminationRequestDate RequestDate { get; init; }

    public TerminationCompleteDate? CompleteDate { get; private set; }

    public void Complete(IDateTime dateTime)
    {
        if (CompleteDate is not null)
        {
            return;
        }

        CompleteDate = TerminationCompleteDate.From(dateTime);
        Card.CompleteStatusChange(CardStatus.Terminated);
        RaiseDomainEvent(new CardTerminatedDomainEvent { CardTermination = this });
    }

    public static void Request(Card card,
        UserId userId,
        TerminationReason reason,
        IDateTime dateTime,
        ICardTerminationRepository cardTerminationRepository)
    {
        if (card.CurrentStatus == CardStatus.Terminated)
        {
            throw new CardDomainException(card.CardId, "Current card status cannot be Terminated.");
        }

        if (card.RequestedStatus == CardStatus.Terminated)
        {
            throw new CardDomainException(card.CardId, "Requested card status cannot be Terminated.");
        }

        if (reason is TerminationReason.Expired)
        {
            throw new CardDomainException(card.CardId, "Termination reason Expired is not allowed.");
        }

        var termination = new CardTermination
        {
            CardId = card.CardId,
            Card = card,
            Reason = reason,
            UserId = userId,
            RequestDate = TerminationRequestDate.From(dateTime)
        };

        cardTerminationRepository.AddCardTermination(termination);
        card.RequestStatus(CardStatus.Terminated);
        termination.RaiseDomainEvent(new CardTerminationRequestedDomainEvent { CardTermination = termination });
    }

    public static async Task Expire(Card card,
        IDateTime dateTime,
        ICardTerminationRepository cardTerminationRepository,
        CancellationToken cancellationToken)
    {
        if (card.CurrentStatus is CardStatus.Terminated)
        {
            return;
        }

        if (card.RequestedStatus is CardStatus.Terminated)
        {
            var requestedTermination = await cardTerminationRepository.SingleAsync(card.CardId, cancellationToken);
            requestedTermination.Complete(dateTime);
            return;
        }

        var expirationTermination = new CardTermination
        {
            CardId = card.CardId,
            Card = card,
            Reason = TerminationReason.Expired,
            UserId = UserId.ExternalIssuer,
            RequestDate = TerminationRequestDate.From(dateTime),
            CompleteDate = TerminationCompleteDate.From(dateTime)
        };

        cardTerminationRepository.AddCardTermination(expirationTermination);
        card.CompleteStatusChange(CardStatus.Terminated);
        expirationTermination.RaiseDomainEvent(
            new CardTerminatedDomainEvent { CardTermination = expirationTermination });
    }
}
