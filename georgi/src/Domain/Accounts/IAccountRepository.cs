namespace Domain.Accounts;

public interface IAccountRepository
{
    Task<Account> SingleAsync(AccountId accountId, CancellationToken cancellationToken);
}
