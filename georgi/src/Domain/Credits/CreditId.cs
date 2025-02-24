namespace Domain.Credits;

public sealed record CreditId
{
    private CreditId() { }

    public required Guid Value { get; init; }

    public static CreditId New() => new() { Value = Guid.CreateVersion7() };
}
