namespace Domain.Credits;

public interface ICreditRepository
{
    Task<Credit> FindAsync(CreditId creditId, CancellationToken cancellationToken);
}
