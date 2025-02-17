using Domain.Accounts;

namespace Domain.Cards;

public interface ILastAccountCardRepository
{
    Task<LastAccountCard> LoadLastAccountCard(AccountId accountId, CancellationToken cancellationToken);
}
