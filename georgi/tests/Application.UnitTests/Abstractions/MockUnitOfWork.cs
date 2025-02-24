using Application.Abstractions;

namespace Application.UnitTests.Abstractions;

public sealed class MockUnitOfWork : IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
