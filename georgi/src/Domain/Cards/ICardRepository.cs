namespace Domain.Cards;

public interface ICardRepository
{
    void AddCard(Card card);

    Task<Card> SingleAsync(CardId cardId, CancellationToken cancellationToken);
}
