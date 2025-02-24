namespace Domain.Cards.Issuance;

public sealed record LastAccountCardIssuance
{
    private LastAccountCardIssuance() { }

    public required CardIssuance? Value { get; init; }
}
