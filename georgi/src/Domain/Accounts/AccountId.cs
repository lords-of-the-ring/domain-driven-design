namespace Domain.Accounts;

public sealed record AccountId
{
    private AccountId() { }

    public required Guid Value { get; init; }

    public static AccountId New() => new() { Value = Guid.CreateVersion7() };
}
