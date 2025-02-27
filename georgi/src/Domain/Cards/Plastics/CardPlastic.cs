using Domain.Abstractions;
using Domain.Cards.Plastics.Locations;

namespace Domain.Cards.Plastics;

public sealed class CardPlastic : DomainEntity
{
    private CardPlastic() { }

    public required CardId CardId { get; init; }

    public required PlasticLocationId LocationId { get; init; }

    public static void Create(Card card, ICardPlasticRepository cardPlasticRepository)
    {
        if (!card.Type.SupportsPlastic())
        {
            return;
        }

        var plastic = new CardPlastic { CardId = card.CardId, LocationId = PlasticLocationId.Manufacturer };

        cardPlasticRepository.AddCardPlastic(plastic);
    }
}
