using Application.Abstractions;

using Domain.Cards.Plastics;

namespace Application.Features.Cards.Plastics.ChangeLocation;

public sealed class ChangePlasticLocationCommandHandler(
    CommandHandlerArguments arguments,
    ICardPlasticRepository cardPlasticRepository
) : CommandHandler<ChangePlasticLocationCommand, bool>(arguments)
{
    protected override async Task<bool> Execute(ChangePlasticLocationCommand command,
        CancellationToken cancellationToken)
    {
        var plastic = await cardPlasticRepository.SingleOrDefaultAsync(command.CardId, cancellationToken);

        if (plastic is null)
        {
            throw new ApplicationException($"Plastic for card with ID '{command.CardId.Value}' not found.");
        }

        plastic.ChangeLocation();

        return true;
    }
}
