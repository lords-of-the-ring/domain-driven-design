using Application.Abstractions;

using Domain.Abstractions;
using Domain.Cards;
using Domain.Cards.Termination;
using Domain.Users;

namespace Application.Features.Cards.Termination.Request;

public sealed class RequestCardTerminationCommandHandler(
    CommandHandlerArguments arguments,
    ICardRepository cardRepository,
    ICardTerminationRepository cardTerminationRepository,
    ICurrentUserService currentUserService,
    IDateTime dateTime
) : CommandHandler<RequestCardTerminationCommand, bool>(arguments)
{
    protected override async Task<bool> Execute(RequestCardTerminationCommand command,
        CancellationToken cancellationToken)
    {
        var card = await cardRepository.SingleAsync(command.CardId, cancellationToken);

        var userId = currentUserService.GetUserId();

        CardTermination.Request(
            card,
            userId,
            command.TerminationReason,
            dateTime,
            cardTerminationRepository);

        return true;
    }
}
