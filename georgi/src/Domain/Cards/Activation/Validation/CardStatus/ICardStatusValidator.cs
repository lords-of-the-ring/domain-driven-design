using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.CardStatus;

public interface ICardStatusValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
