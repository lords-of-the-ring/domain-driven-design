using Domain.Accounts;
using Domain.Credits.QuickMoney;

using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.QuickMoney;

public sealed class QuickMoneyValidator(
    ICardRepository cardRepository,
    IAccountRepository accountRepository,
    ICreditQuickMoneyRepository creditQuickMoneyRepository
) : IQuickMoneyValidator
{
    public async Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken)
    {
        var card = await cardRepository.SingleAsync(cardId, cancellationToken);

        var account = await accountRepository.SingleAsync(card.AccountId, cancellationToken);

        var quickMoney = await creditQuickMoneyRepository.SingleOrDefaultAsync(account.CreditId, cancellationToken);

        if (quickMoney is null)
        {
            return new Success();
        }

        if (quickMoney.IsDeclined)
        {
            return new Success();
        }

        return Error.Validation(Errors.ActivationNotAllowedByQuickMoney);
    }

    public static class Errors
    {
        public const string ActivationNotAllowedByQuickMoney = "Activation is not allowed by quick money";
    }
}
