namespace Domain.Cards.Issuers;

public sealed class CardIssuer
{
    private CardIssuer() { }

    public required CardIssuerId IssuerId { get; init; }

    public required CardIssuerName Name { get; init; }

    public static CardIssuer Create(CardIssuerId issuerId, CardIssuerName name) =>
        new() { IssuerId = issuerId, Name = name };
}
