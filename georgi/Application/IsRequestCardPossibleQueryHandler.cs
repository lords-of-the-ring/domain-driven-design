using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuance;
using Domain.Credits;

using ErrorOr;

namespace Application;

public class IsRequestCardPossibleQueryHandler(
    ILastAccountCardRepository lastAccountCardRepository,
    IAccountRepository accountRepository,
    ICreditRepository creditRepository,
    IAccountBlockInfoRepository accountBlockInfoRepository,
    IDateTime dateTime)
{
    public async Task<ErrorOr<Success>> Execute(IsRequestCardPossibleQuery query,
        CancellationToken cancellationToken)
    {
        var lastAccountCard = await lastAccountCardRepository.LoadLastAccountCard(query.AccountId, cancellationToken);

        if (lastAccountCard.Value is null)
        {
            var account = await accountRepository.FindAsync(query.AccountId, cancellationToken);
            var credit = await creditRepository.FindAsync(account.CreditId, cancellationToken);
            return CardIssuance.CheckIfRequestingInitialCardIsAllowed(credit);
        }

        var accountBlockInfo = await accountBlockInfoRepository.LoadAccountBlockInfo(
            query.AccountId,
            cancellationToken);

        return CardIssuance.CheckIfRequestingRenewedCardIsAllowed(accountBlockInfo, lastAccountCard, dateTime);
    }
}
