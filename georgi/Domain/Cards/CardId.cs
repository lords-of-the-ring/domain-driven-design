namespace Domain.Cards;

public sealed record CardId
{
    private CardId() { }

    public required Guid Value { get; init; }

    public static CardId New() => new() { Value = Guid.CreateVersion7() };
}
