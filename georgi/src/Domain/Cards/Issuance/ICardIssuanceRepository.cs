namespace Domain.Cards.Issuance;

public interface ICardIssuanceRepository
{
    void AddCardIssuance(CardIssuance cardIssuance);

    Task<CardIssuance> SingleAsync(CardId cardId, CancellationToken cancellationToken);
}
