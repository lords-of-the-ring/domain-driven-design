using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation;

public interface IRequestCardActivationValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
