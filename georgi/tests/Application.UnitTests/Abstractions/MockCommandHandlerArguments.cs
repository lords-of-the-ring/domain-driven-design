using Application.Abstractions;

namespace Application.UnitTests.Abstractions;

public static class MockCommandHandlerArguments
{
    public static CommandHandlerArguments Instance => new() { UnitOfWork = new MockUnitOfWork() };
}
