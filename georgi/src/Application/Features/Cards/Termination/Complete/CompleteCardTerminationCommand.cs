using Domain.Cards;

namespace Application.Features.Cards.Termination.Complete;

public sealed record CompleteCardTerminationCommand
{
    public required CardId CardId { get; init; }
}
