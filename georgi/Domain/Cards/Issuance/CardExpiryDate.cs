using Domain.Abstractions;

namespace Domain.Cards.Issuance;

public sealed record CardExpiryDate
{
    public bool IsInPreExpiryPeriod(IDateTime dateTime) => true;

    public bool IsInAfterExpiryPeriod(IDateTime dateTime) => true;
}
