namespace Domain.Cards.Issuance;

public sealed record CardExpiryDate
{
    private CardExpiryDate() { }

    public required DateTimeOffset Value { get; init; }

    public static CardExpiryDate From(DateTimeOffset value) => new() { Value = value };
}
