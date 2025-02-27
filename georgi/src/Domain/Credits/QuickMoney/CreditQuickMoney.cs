using Domain.Abstractions;
using Domain.Users;

namespace Domain.Credits.QuickMoney;

public sealed class CreditQuickMoney : DomainEntity
{
    private CreditQuickMoney() { }

    public required CreditId CreditId { get; init; }

    public required QuickMoneyAmount Amount { get; init; }

    public bool IsDeclined { get; private set; }

    public QuickMoneyDeclineDate? DeclineDate { get; private set; }

    public UserId? DeclineUserId { get; private set; }

    public static void Create(
        CreditId creditId,
        QuickMoneyAmount amount,
        ICreditQuickMoneyRepository quickMoneyRepository)
    {
        var quickMoney = new CreditQuickMoney
        {
            CreditId = creditId,
            Amount = amount,
            IsDeclined = false,
            DeclineDate = null,
            DeclineUserId = null
        };

        quickMoneyRepository.AddCreditQuickMoney(quickMoney);
    }
}
