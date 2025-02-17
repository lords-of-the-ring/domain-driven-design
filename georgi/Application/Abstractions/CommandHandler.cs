namespace Application.Abstractions;

public abstract class CommandHandler<TCommand, TResult>
{
    protected abstract Task<TResult> Execute(TCommand command, CancellationToken cancellationToken);

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute(command, cancellationToken);

        return result;
    }
}
