using Domain.Cards.Activation.Request.Validation.CardPlastic;
using Domain.Cards.Activation.Request.Validation.CardStatus;
using Domain.Cards.Activation.Request.Validation.QuickMoney;

using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation;

public sealed class RequestCardActivationValidator(
    ICardStatusValidator cardStatusValidator,
    ICardPlasticValidator cardPlasticValidator,
    IQuickMoneyValidator quickMoneyValidator
) : IRequestCardActivationValidator
{
    public async Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken)
    {
        var cardStatusResult = await cardStatusValidator.Validate(cardId, cancellationToken);

        if (cardStatusResult.IsError)
        {
            return cardStatusResult;
        }

        var cardPlasticResult = await cardPlasticValidator.Validate(cardId, cancellationToken);

        if (cardPlasticResult.IsError)
        {
            return cardPlasticResult;
        }

        var quickMoneyResult = await quickMoneyValidator.Validate(cardId, cancellationToken);

        return quickMoneyResult.IsError ? quickMoneyResult : new Success();
    }
}
