using ErrorOr;

namespace Domain.Cards.Activation.Validation;

public interface IRequestCardActivationValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
