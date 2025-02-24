namespace Domain.Cards.Issuance;

public sealed record CardPan
{
    private CardPan() { }

    public required string Value { get; init; }

    public static CardPan From(string value) => new() { Value = value };
}
