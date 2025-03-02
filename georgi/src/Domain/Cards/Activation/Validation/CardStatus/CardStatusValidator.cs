using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.CardStatus;

public sealed class CardStatusValidator(ICardRepository cardRepository) : ICardStatusValidator
{
    public async Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken)
    {
        var card = await cardRepository.SingleAsync(cardId, cancellationToken);

        if (card.CurrentStatus != Cards.CardStatus.Issued)
        {
            return Error.Validation(Errors.CurrentCardStatusMustBeIssued);
        }

        if (card.RequestedStatus is not null)
        {
            return Error.Validation(Errors.AnotherStatusIsAlreadyRequested);
        }

        return new Success();
    }

    private static class Errors
    {
        public const string CurrentCardStatusMustBeIssued = "Current card status must be Issued";
        public const string AnotherStatusIsAlreadyRequested = "Another card status is already requested";
    }
}
