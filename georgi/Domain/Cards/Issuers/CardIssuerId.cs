namespace Domain.Cards.Issuers;

public sealed record CardIssuerId
{
    private CardIssuerId() { }

    public required byte Value { get; init; }

    public static CardIssuerId From(byte value) => new() { Value = value };
}
