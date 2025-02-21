namespace Domain.Cards.Issuers;

public sealed record CardIssuerName
{
    private CardIssuerName() { }

    public required string Value { get; init; }

    public static CardIssuerName Create(string value) => new() { Value = value };
}
