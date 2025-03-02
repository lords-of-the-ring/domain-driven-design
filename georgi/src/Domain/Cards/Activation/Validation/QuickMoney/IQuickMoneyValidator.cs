using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.QuickMoney;

public interface IQuickMoneyValidator
{
    Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken);
}
