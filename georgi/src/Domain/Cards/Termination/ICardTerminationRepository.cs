namespace Domain.Cards.Termination;

public interface ICardTerminationRepository
{
    Task<CardTermination> SingleAsync(CardId cardId, CancellationToken cancellationToken);

    void AddCardTermination(CardTermination cardTermination);
}
