namespace Domain.Accounts;

public sealed class AccountDomainException(AccountId accountId, string reason) : Exception
{
    public AccountId AccountId { get; } = accountId;

    public string Reason { get; } = reason;
}
