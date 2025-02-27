using Domain.Abstractions;

namespace Domain.Credits.QuickMoney;

public sealed record QuickMoneyDeclineDate : ValueObject<DateTimeOffset>
{
    private QuickMoneyDeclineDate() { }

    public static QuickMoneyDeclineDate From(IDateTime dateTime) => new() { Value = dateTime.UtcNow };
}
