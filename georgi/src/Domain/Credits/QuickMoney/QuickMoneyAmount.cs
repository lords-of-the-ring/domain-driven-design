using Domain.Abstractions;

namespace Domain.Credits.QuickMoney;

public sealed record QuickMoneyAmount : ValueObject<decimal>
{
    private QuickMoneyAmount() { }

    public static QuickMoneyAmount From(decimal value) => new() { Value = Math.Abs(value) };
}
