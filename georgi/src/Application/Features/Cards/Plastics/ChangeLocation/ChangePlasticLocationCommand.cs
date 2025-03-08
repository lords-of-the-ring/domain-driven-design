using Domain.Cards;
using Domain.Cards.Plastics.Locations;

namespace Application.Features.Cards.Plastics.ChangeLocation;

public sealed record ChangePlasticLocationCommand
{
    public required CardId CardId { get; init; }

    public required PlasticLocationId NewLocationId { get; init; }
}
