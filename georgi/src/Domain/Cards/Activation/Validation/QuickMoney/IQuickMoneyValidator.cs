using ErrorOr;

namespace Domain.Cards.Activation.Validation.QuickMoney;

public interface IQuickMoneyValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
