namespace Domain.Accounts;

public interface IAccountRepository
{
    Task<Account> FindAsync(AccountId accountId, CancellationToken cancellationToken);
}
