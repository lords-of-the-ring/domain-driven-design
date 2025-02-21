using Application.Abstractions;

using Domain.Abstractions;
using Domain.Cards.Issuance;

namespace Application.Features.Cards.Issuance.CompleteCardIssuance;

public sealed class CompleteCardIssuanceCommandHandler(
    CommandHandlerArguments arguments,
    IDateTime dateTime,
    ICardIssuanceRepository cardIssuanceRepository
) : CommandHandler<CompleteCardIssuanceCommand, bool>(arguments)
{
    protected override async Task<bool> Execute(CompleteCardIssuanceCommand command,
        CancellationToken cancellationToken)
    {
        var cardIssuance = await cardIssuanceRepository.SingleAsync(command.CardId, cancellationToken);

        cardIssuance.Complete(dateTime, command.CardExpiryDate, command.CardPan);

        return true;
    }
}
