namespace Domain.Accounts;

public sealed class AccountBlockInfo
{
    public required bool HasPendingBlocks { get; init; }

    private AccountBlockInfo() { }
}
