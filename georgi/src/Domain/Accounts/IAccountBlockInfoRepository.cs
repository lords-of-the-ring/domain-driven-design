namespace Domain.Accounts;

public interface IAccountBlockInfoRepository
{
    Task<AccountBlockInfo> LoadAccountBlockInfo(AccountId accountId, CancellationToken cancellationToken);
}
