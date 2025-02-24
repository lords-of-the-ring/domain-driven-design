using Domain.Cards;
using Domain.Cards.Termination;

namespace Application.Features.Cards.Termination.Request;

public sealed record RequestCardTerminationCommand
{
    public required CardId CardId { get; init; }

    public required TerminationReason TerminationReason { get; init; }
}
