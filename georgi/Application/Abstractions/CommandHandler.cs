namespace Application.Abstractions;

public abstract class CommandHandler<TCommand, TResult>(CommandHandlerArguments arguments)
{
    protected abstract Task<TResult> Execute(TCommand command, CancellationToken cancellationToken);

    protected virtual bool WrapInsideTransaction => false;

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute(command, cancellationToken);

        await arguments.UnitOfWork.SaveChanges(cancellationToken);

        return result;
    }
}
