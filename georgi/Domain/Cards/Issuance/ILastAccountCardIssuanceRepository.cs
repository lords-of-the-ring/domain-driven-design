using Domain.Accounts;

namespace Domain.Cards.Issuance;

public interface ILastAccountCardIssuanceRepository
{
    Task<LastAccountCardIssuance> Load(AccountId accountId, CancellationToken cancellationToken);
}
