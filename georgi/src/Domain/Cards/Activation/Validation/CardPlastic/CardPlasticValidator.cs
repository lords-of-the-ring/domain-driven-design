using Domain.Cards.Plastics;
using Domain.Cards.Plastics.Locations;
using Domain.Cards.Plastics.Locations.Rules;

using ErrorOr;

namespace Domain.Cards.Activation.Request.Validation.CardPlastic;

public sealed class CardPlasticValidator(
    ICardPlasticRepository cardPlasticRepository,
    IPlasticLocationRuleRepository plasticLocationRuleRepository) : ICardPlasticValidator
{
    public async Task<ErrorOr<Success>> Validate(CardId cardId, CancellationToken cancellationToken)
    {
        var plastic = await cardPlasticRepository.SingleOrDefaultAsync(cardId, cancellationToken);

        if (plastic is null)
        {
            return new Success();
        }

        var plasticLocationRule = await plasticLocationRuleRepository.SingleOrDefaultAsync(
            plastic.LocationId,
            PlasticLocationId.Customer,
            cancellationToken);

        if (plasticLocationRule is null)
        {
            return Error.Validation(Errors.ActivationNotAllowedByLocationRules);
        }

        return new Success();
    }

    private static class Errors
    {
        public const string ActivationNotAllowedByLocationRules = "Activation not allowed by plastic location rules";
    }
}
