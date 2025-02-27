namespace Domain.Credits.QuickMoney;

public interface ICreditQuickMoneyRepository
{
    Task<CreditQuickMoney?> SingleOrDefaultAsync(CreditId creditId, CancellationToken cancellationToken);

    void AddCreditQuickMoney(CreditQuickMoney creditQuickMoney);
}
