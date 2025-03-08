using ErrorOr;

namespace Domain.Cards.Activation.Validation.CardStatus;

public interface ICardStatusValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
