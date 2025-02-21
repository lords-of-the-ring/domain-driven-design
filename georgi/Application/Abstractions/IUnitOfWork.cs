namespace Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChanges(CancellationToken cancellationToken);
}
