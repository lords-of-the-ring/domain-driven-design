using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.CardPlastic;

public interface ICardPlasticValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
