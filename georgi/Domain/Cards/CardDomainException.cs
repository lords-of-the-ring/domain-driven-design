namespace Domain.Cards;

public sealed class CardDomainException(CardId cardId, string reason)
    : Exception($"An unexpected error for card with id '{cardId.Value}' has occurred: {reason}.")
{
    public CardId CardId { get; } = cardId;

    public string Reason { get; } = reason;
}
