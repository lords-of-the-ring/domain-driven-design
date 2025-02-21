namespace Domain.Credits;

public interface ICreditRepository
{
    Task<Credit> SingleAsync(CreditId creditId, CancellationToken cancellationToken);
}
