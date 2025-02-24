namespace Domain.Accounts;

public sealed class AccountBlockInfo
{
    private AccountBlockInfo() { }

    public required bool HasPendingBlocks { get; init; }
}
