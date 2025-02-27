namespace Domain.Cards.Plastics.Locations.Rules;

public interface IPlasticLocationRuleRepository
{
    Task<PlasticLocationRule?> SingleOrDefaultAsync(PlasticLocationId from, PlasticLocationId to,
        CancellationToken cancellationToken);
}
