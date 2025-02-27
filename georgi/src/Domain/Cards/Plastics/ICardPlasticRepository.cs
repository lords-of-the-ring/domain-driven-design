namespace Domain.Cards.Plastics;

public interface ICardPlasticRepository
{
    void AddCardPlastic(CardPlastic cardPlastic);

    Task<CardPlastic?> SingleOrDefaultAsync(CardId cardId, CancellationToken cancellationToken);
}
