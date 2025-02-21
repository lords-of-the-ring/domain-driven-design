namespace Application.Abstractions;

public sealed class CommandHandlerArguments
{
    public required IUnitOfWork UnitOfWork { get; init; }
}
