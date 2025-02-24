namespace Domain.Accounts;

public sealed class AccountDomainException(AccountId accountId, string reason)
    : Exception($"An unexpected error for card with id '{accountId.Value}' has occurred: {reason}.")
{
    public AccountId AccountId { get; } = accountId;

    public string Reason { get; } = reason;
}
