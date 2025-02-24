using Domain.Cards;
using Domain.Cards.Issuance;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Cards.Issuance;

public sealed class CardIssuanceRepository(DbContext dbContext) : ICardIssuanceRepository
{
    public void AddCardIssuance(CardIssuance cardIssuance)
    {
        dbContext.Set<CardIssuance>().Add(cardIssuance);
    }

    public Task<CardIssuance> SingleAsync(CardId cardId, CancellationToken cancellationToken)
    {
        return dbContext
            .Set<CardIssuance>()
            .Include(x => x.Card)
            .SingleAsync(x => x.CardId == cardId, cancellationToken);
    }
}
