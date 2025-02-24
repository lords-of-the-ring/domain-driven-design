using Application.Abstractions;

using Domain.Abstractions;
using Domain.Cards.Termination;

namespace Application.Features.Cards.Termination.Complete;

public sealed class CompleteCardTerminationCommandHandler(
    CommandHandlerArguments arguments,
    ICardTerminationRepository cardTerminationRepository,
    IDateTime dateTime
) : CommandHandler<CompleteCardTerminationCommand, bool>(arguments)
{
    protected override async Task<bool> Execute(CompleteCardTerminationCommand command,
        CancellationToken cancellationToken)
    {
        var cardTermination = await cardTerminationRepository.SingleAsync(command.CardId, cancellationToken);

        cardTermination.Complete(dateTime);

        return true;
    }
}
