using Domain.Abstractions;
using Domain.Cards.Activation.Validation;
using Domain.Users;

namespace Domain.Cards.Activation;

public sealed class CardActivation : DomainEntity
{
    private CardActivation() { }

    public required CardId CardId { get; init; }

    public required Card Card { get; init; }

    public required UserId UserId { get; init; }

    public required ActivationRequestDate RequestDate { get; init; }

    public ActivationCompleteDate? CompleteDate { get; private set; }

    public void Complete(IDateTime dateTime)
    {
        if (CompleteDate is not null)
        {
            return;
        }

        CompleteDate = ActivationCompleteDate.From(dateTime);
        Card.CompleteStatusChange(CardStatus.Active);
        RaiseDomainEvent(new CardActivatedDomainEvent());
    }

    public static async Task Request(
        Card card,
        UserId userId,
        IDateTime dateTime,
        IRequestCardActivationValidator activationValidator,
        ICardActivationRepository cardActivationRepository,
        CancellationToken cancellationToken)
    {
        var validationResult = await activationValidator.Validate(card.CardId, cancellationToken);

        if (validationResult.IsError)
        {
            throw new CardDomainException(card.CardId, validationResult.FirstError.Code);
        }

        var activation = new CardActivation
        {
            CardId = card.CardId,
            Card = card,
            UserId = userId,
            RequestDate = ActivationRequestDate.From(dateTime),
            CompleteDate = null
        };

        cardActivationRepository.AddCardActivation(activation);
        card.RequestStatusChange(CardStatus.Active);
        activation.RaiseDomainEvent(new CardActivationRequestedDomainEvent { CardActivation = activation });
    }
}
