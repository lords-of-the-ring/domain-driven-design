using Domain.Abstractions;

namespace Domain.Cards.Plastics.Locations;

public sealed record PlasticLocationId : ValueObject<int>
{
    public static readonly PlasticLocationId Manufacturer = new() { Value = 1 };
    public static readonly PlasticLocationId Customer = new() { Value = 2 };
    private PlasticLocationId() { }
};
